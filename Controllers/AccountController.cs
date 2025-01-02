using Microsoft.AspNetCore.Mvc;
using AppointmentHospital.Services;
using static AppointmentHospital.DTOs.Account.AccountRequest;
using Microsoft.AspNetCore.Identity;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Hangfire;
using AppointmentHospital.Services.Implement;
using ResetPasswordRequest = AppointmentHospital.DTOs.Account.AccountRequest.ResetPasswordRequest;
using Microsoft.Extensions.Options;
using AppointmentHospital.Configuration.BaseUrl;
using System.Security.Claims;

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
        public AccountController(IAccountService accountService, IOptions<BaseUrl> options ,IEmailService emailService, IHttpContextAccessor contextAccessor, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _accountService = accountService;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _options = options;
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
            return View(new RegisterUserRequest() { Email = string.Empty, FullName = string.Empty, Password = string.Empty, ConfirmPassword = string.Empty, Address = string.Empty });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            if (!await _accountService.LoginAsync(request))
            {
                var userFounded = await _userManager.FindByEmailAsync(request.Email);
                await SendMail(userFounded);
                return View("ConfirmEmail", request.Email);
            }
            if (_contextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "ManagingPatient", new { area = "Admin" });
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && _contextAccessor.HttpContext != null && _contextAccessor.HttpContext.User.IsInRole("Patient"))
            {
                _contextAccessor.HttpContext.Session.SetString("PatientId", user.Id.ToString());
                return RedirectToAction("Index", "Patient");
            }
            if (user != null && _contextAccessor.HttpContext != null && _contextAccessor.HttpContext.User.IsInRole("Doctor"))
            {
                _contextAccessor.HttpContext.Session.SetString("DoctorId", user.Id.ToString());
                return RedirectToAction("Index", "Doctor");
            }
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> ExternalLogin(string provider)
        {
            var externalList = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var externalAuthen = externalList.Find(e => e.Name == provider);
            if (externalAuthen == null)
            {
                return NotFound("Cannot find authenticated provider" + provider);
            }
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var configureExternalLogin = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, configureExternalLogin);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            string externalMail = null;
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                return RedirectToAction("Login");
            }
            var loginResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if(loginResult.Succeeded)
            {
                var userId = _accountService.GetIdByEmail(info.Principal.FindFirstValue(ClaimTypes.Email));
                _contextAccessor.HttpContext.Session.SetString("PatientId", userId.ToString());
                return RedirectToAction("Index", "Patient");
            }
            else
            {
                //var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    externalMail = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "null";
                }
                var user = await _userManager.FindByEmailAsync(externalMail);
                if (user == null)
                {
                    if(user == null)
                    {
                        var newUser = new User
                        {
                            UserName = externalMail,
                            Email = externalMail
                        };
                        var createResult = await _userManager.CreateAsync(newUser);
                        if (createResult.Succeeded)
                        {
                            var addLoginNewUserResult = await _userManager.AddLoginAsync(newUser, info);
                            if (addLoginNewUserResult.Succeeded)
                            {
                                var tokenNewUser = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                                await _userManager.ConfirmEmailAsync(newUser, tokenNewUser);
                                await _signInManager.SignInAsync(newUser, isPersistent: false);
                                return RedirectToAction("Index", "Patient");
                            }
                        }
                    }
                }
                //Existed user but dont confiremed email -> Confirmed email - Link
                if (!( await _userManager.IsEmailConfirmedAsync(user)))
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var identityResult = await _userManager.ConfirmEmailAsync(user, token);
                    if (!identityResult.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
                    if (!addLoginResult.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                    return RedirectToAction("Index", "Patient");
                }
                //Existed user but dont link with external provider
                var addResult=  await _userManager.AddLoginAsync(user, info);
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
                if(!addResult.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                return RedirectToAction("Index", "Patient");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var user = await _accountService.RegisterAsync(request);
            await SendMail(user);
            return View("ConfirmEmail", request.Email);
        }

        public async Task<IActionResult> VerifyEmail(string token, string userId)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token is missing");
            }
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("UserId is missing");
            }
            var user = await _userManager.FindByIdAsync(userId);
            var tokenDecode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, tokenDecode);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to confirm email");
            }
            return View("ConfirmedEmail");
        }
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                ModelState.AddModelError("User", "User không tồn tại");
                return View(request);
            }
            if (!(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return View("UnconfirmedEmail", new UnconfirmedEmailRequest { Email = user.Email });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = Url.Action("ResetPassword", "Account", new { code = code, email = request.EmailAddress });
            Console.WriteLine(_options.Value.LocalHost);
            var baseUrl = $"{_options.Value.LocalHost}{url}";
            var body = await _emailService.GetResetPasswordTemplate(user.UserName, baseUrl);
            BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMailAsync(request.EmailAddress, "Đặt lại mật khẩu", body));
            return View("ConfirmEmail", request.EmailAddress);
        }

        public IActionResult ResetPassword(string code, string email)
        {
            if(code == null)
            {
                return NotFound("Cannot find the token");
            }
            var tokenDecoding = WebEncoders.Base64UrlDecode(code);
            var token = Encoding.UTF8.GetString(tokenDecoding);
            return View(new ResetPasswordRequest { Code = token, Email = email });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                ModelState.AddModelError("ResetPassword", "Cannot find user");
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("ResetPassword", "Cannot reset pasword");
                return View();
            }
            return View("Login", new LoginUserRequest { Email = string.Empty, Password = string.Empty});
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (email == null)
            {
                return NotFound("Email is empty");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User isn't exist");
            }
            await SendMail(user);
            return View("ConfirmEmail", email);
        }
        private  async Task SendMail(User user)
        {
            var tokenConfirmedEmail = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirmedEmail));
            var urlBase = Url.Action("VerifyEmail", "Account", new { token = token, userId = user.Id });
            var url = $"{_options.Value.LocalHost}{urlBase}";
            var body = await _emailService.GetConfirmedEmailTemplate(user.UserName, url);
            BackgroundJob.Enqueue<IEmailService>(es => es.SendMailAsync(user.Email, "Xác thực email", body));
        }
    }
}
