// AccountController.cs - Đã cập nhật để tự động dùng BaseUrl phù hợp cho LocalHost và Production

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

        public AccountController(
            IAccountService accountService,
            AppDbContext appDbContext,
            IOptions<BaseUrl> options,
            IEmailService emailService,
            IHttpContextAccessor contextAccessor,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _accountService = accountService;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _options = options;
            _appDbContext = appDbContext;
        }

        private string GetBaseUrl()
        {
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
            var externalList = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var externalAuthen = externalList.Find(e => e.Name == provider);
            if (externalAuthen == null)
            {
                return NotFound("Cannot find authenticated provider " + provider);
            }
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", null, Request.Scheme);
            var configureExternalLogin = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, configureExternalLogin);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            string externalMail = null;
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var loginResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (loginResult.Succeeded)
            {
                var userId = _accountService.GetIdByEmail(info.Principal.FindFirstValue(ClaimTypes.Email));
                _contextAccessor.HttpContext.Session.SetString("PatientId", userId.ToString());
                return RedirectToAction("Index", "Patient");
            }

            externalMail = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(externalMail);

            if (user == null)
            {
                var newUser = new User { UserName = externalMail, Email = externalMail };
                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded) return RedirectToAction("Login");

                var patient = new Patient { FullName = newUser.UserName, User = newUser };
                await _userManager.AddToRoleAsync(newUser, "Patient");
                await _appDbContext.AddAsync(patient);
                await _appDbContext.SaveChangesAsync();

                var addLoginNewUserResult = await _userManager.AddLoginAsync(newUser, info);
                if (addLoginNewUserResult.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    await _userManager.ConfirmEmailAsync(newUser, token);
                    await _signInManager.SignInAsync(newUser, false);
                    _contextAccessor.HttpContext.Session.SetString("PatientId", newUser.Id.ToString());
                    return RedirectToAction("Index", "Patient");
                }
            }
            else
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
                _contextAccessor.HttpContext.Session.SetString("PatientId", user.Id.ToString());
                return RedirectToAction("Index", "Patient");
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var (user, response) = await _accountService.RegisterAsync(request);
            
            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(request);
            }

            await SendMail(user);
            return View("ConfirmEmail", request.Email);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                throw new Exception("Token or UserId is missing");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var tokenDecode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, tokenDecode);

            if (!result.Succeeded)
                throw new Exception("Failed to confirm email");

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
            var resetUrl = Url.Action("ResetPassword", "Account", new { code = code, email = request.EmailAddress }, Request.Scheme);
            var fullUrl = resetUrl; // Use the fully qualified URL directly

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
            var url = Url.Action("VerifyEmail", "Account", new { token = encodedToken, userId = user.Id }, Request.Scheme);
            var fullUrl = url; // Use the fully qualified URL directly
            var body = await _emailService.GetConfirmedEmailTemplate(user.UserName, fullUrl);
            BackgroundJob.Enqueue<IEmailService>(es => es.SendMailAsync(user.Email, "Xác thực email", body));
        }

        [AllowAnonymous]
        public IActionResult AccessDeny()
        {
            return View();
        }
    }
}
