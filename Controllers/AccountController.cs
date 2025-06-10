using Microsoft.AspNetCore.Mvc;
using AppointmentHospital.Services;
using static AppointmentHospital.DTOs.Account.AccountRequest;
using Microsoft.AspNetCore.Identity;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Hangfire;
using AppointmentHospital.Services.Implement;
using Microsoft.Extensions.Options;
using AppointmentHospital.Configuration.BaseUrl;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AppointmentHospital.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IOptions<BaseUrl> _options;
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            AppDbContext appDbContext,
            IOptions<BaseUrl> options,
            IEmailService emailService,
            IHttpContextAccessor contextAccessor,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _options = options;
            _appDbContext = appDbContext;
            _logger = logger;
        }

        private string GetBaseUrl()
        {
            var request = _contextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var scheme = request.Scheme;
                var host = request.Host.Value;
                return $"{scheme}://{host}";
            }
            return Request.Host.Host.Contains("localhost") ? _options.Value.LocalHost : _options.Value.Production;
        }

        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordRequest { EmailAddress = string.Empty });
        }

        public async Task<IActionResult> Login()
        {
            return View(new LoginUserRequest() { Email = string.Empty, Password = string.Empty });
        }

        public IActionResult Register()
        {
            return View(new RegisterUserRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _accountService.LoginAsync(request);
            if (result.Status == 404 || result.Message == "Cannot find user")
            {
                ModelState.AddModelError("", "User not found. Please check your email address.");
                return View(request);
            }

            if (result.Status == 403)
            {
                ModelState.AddModelError("", "Please confirm your email before logging in.");
                var userFounded = await _userManager.FindByEmailAsync(request.Email);
                if (userFounded != null)
                {
                    await SendMail(userFounded);
                }
                return View(request);
            }

            if (result.Status == 400)
            {
                ModelState.AddModelError("", "Incorrect password. Please try again.");
                return View(request);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found after login. Please contact support.");
                return View(request);
            }

            var context = _contextAccessor.HttpContext;
            if (context != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "ManagingPatient", new { area = "Admin" });
                if (await _userManager.IsInRoleAsync(user, "Patient"))
                {
                    context.Session.SetString("PatientId", user.Id.ToString());
                    return RedirectToAction("Index", "Patient");
                }
                if (await _userManager.IsInRoleAsync(user, "Doctor"))
                {
                    context.Session.SetString("DoctorId", user.Id.ToString());
                    return RedirectToAction("Index", "Doctor");
                }
                if (await _userManager.IsInRoleAsync(user, "Collaborator"))
                {
                    context.Session.SetString("CollaboratorId", user.Id.ToString());
                    return RedirectToAction("Index", "Collaborator");
                }
            }

            ModelState.AddModelError("", "Unable to determine user role. Please contact support.");
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> ExternalLogin(string provider)
        {
            try
            {
                var externalList = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                var externalAuthen = externalList.Find(e => e.Name == provider);
                if (externalAuthen == null)
                {
                    _logger.LogWarning("External authentication provider {Provider} not found", provider);
                    return NotFound("Cannot find authenticated provider " + provider);
                }

                // Ensure proper redirect URL with full domain
                var redirectUrl = Url.Action("ExternalLoginCallback", "Account", null, Request.Scheme);
                _logger.LogInformation("External login redirect URL: {RedirectUrl}", redirectUrl);
                
                var configureExternalLogin = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return new ChallengeResult(provider, configureExternalLogin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExternalLogin for provider {Provider}", provider);
                TempData["ErrorMessage"] = "An error occurred during external login. Please try again.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("External login info is null - correlation may have failed");
                    TempData["ErrorMessage"] = "External login failed. Please try again.";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("External login callback received for provider: {Provider}", info.LoginProvider);

                var loginResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
                if (loginResult.Succeeded)
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var userId = _accountService.GetIdByEmail(email);
                    _contextAccessor.HttpContext.Session.SetString("PatientId", userId.ToString());
                    return RedirectToAction("Index", "Patient");
                }

                var externalMail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(externalMail))
                {
                    _logger.LogWarning("External login did not provide email claim");
                    TempData["ErrorMessage"] = "Unable to retrieve email from external provider.";
                    return RedirectToAction("Login");
                }

                var user = await _userManager.FindByEmailAsync(externalMail);

                if (user == null)
                {
                    // Create new user
                    var newUser = new User 
                    { 
                        UserName = externalMail, 
                        Email = externalMail,
                        EmailConfirmed = true // Auto-confirm for external logins
                    };
                    
                    var createResult = await _userManager.CreateAsync(newUser);
                    if (!createResult.Succeeded) 
                    {
                        _logger.LogError("Failed to create user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        TempData["ErrorMessage"] = "Failed to create user account.";
                        return RedirectToAction("Login");
                    }

                    var patient = new Patient { FullName = newUser.UserName, User = newUser };
                    await _userManager.AddToRoleAsync(newUser, "Patient");
                    await _appDbContext.AddAsync(patient);
                    await _appDbContext.SaveChangesAsync();

                    var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(newUser, false);
                        _contextAccessor.HttpContext.Session.SetString("PatientId", newUser.Id.ToString());
                        return RedirectToAction("Index", "Patient");
                    }
                    else
                    {
                        _logger.LogError("Failed to add external login: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // Existing user
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded || addLoginResult.Errors.Any(e => e.Code == "LoginAlreadyAssociated"))
                    {
                        await _signInManager.SignInAsync(user, false);
                        _contextAccessor.HttpContext.Session.SetString("PatientId", user.Id.ToString());
                        return RedirectToAction("Index", "Patient");
                    }
                    else
                    {
                        _logger.LogError("Failed to add external login to existing user: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ExternalLoginCallback");
                TempData["ErrorMessage"] = "An error occurred during external login. Please try again.";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var user = await _accountService.RegisterAsync(request);
            await SendMail(user);
            return View("ConfirmEmail", request.Email);
        }

        public async Task<IActionResult> VerifyEmail(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                throw new Exception("Token or UserId is missing");

            var user = await _userManager.FindByIdAsync(userId);
            var tokenDecode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, tokenDecode);

            if (!result.Succeeded) throw new Exception("Failed to confirm email");
            return View("ConfirmedEmail");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Patient");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                ModelState.AddModelError("User", "User không tồn tại");
                return View(request);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return View("UnconfirmedEmail", new UnconfirmedEmailRequest { Email = user.Email });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = Url.Action("ResetPassword", "Account", new { code = code, email = request.EmailAddress });
            var fullUrl = $"{GetBaseUrl()}{resetUrl}";

            var body = await _emailService.GetResetPasswordTemplate(user.UserName, fullUrl);
            BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMailAsync(request.EmailAddress, "Đặt lại mật khẩu", body));
            return View("ConfirmEmail", request.EmailAddress);
        }

        public IActionResult ResetPassword(string code, string email)
        {
            if (code == null) return NotFound("Cannot find the token");

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            return View(new ResetPasswordRequest { Code = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                ModelState.AddModelError("ResetPassword", "Cannot find user");
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("ResetPassword", "Cannot reset password");
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (email == null) return NotFound("Email is empty");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User isn't exist");

            await SendMail(user);
            return View("ConfirmEmail", email);
        }

        private async Task SendMail(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = Url.Action("VerifyEmail", "Account", new { token = encodedToken, userId = user.Id });
            var fullUrl = $"{GetBaseUrl()}{url}";
            var body = await _emailService.GetConfirmedEmailTemplate(user.UserName, fullUrl);
            BackgroundJob.Enqueue<IEmailService>(es => es.SendMailAsync(user.Email, "Xác thực email", body));
        }

        [AllowAnonymous]
        public IActionResult AccessDeny()
        {
            return View();
        }

        // Add this action to handle external login errors
        [AllowAnonymous]
        public IActionResult ExternalLoginError()
        {
            ViewBag.ErrorMessage = "An error occurred during external authentication. Please try again or use regular login.";
            return View("Login", new LoginUserRequest { Email = string.Empty, Password = string.Empty });
        }
    }
}