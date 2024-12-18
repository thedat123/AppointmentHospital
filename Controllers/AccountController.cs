using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using AppointmentHospital.DTOs.Account;
using AppointmentHospital.Services;
using static AppointmentHospital.DTOs.Account.AccountRequest;
using Microsoft.AspNetCore.Identity;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Hangfire;
using AppointmentHospital.Services.Implement;
using ResetPasswordRequest = AppointmentHospital.DTOs.Account.AccountRequest.ResetPasswordRequest;

namespace AppointmentHospital.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        public AccountController(IAccountService accountService, IEmailService emailService, IHttpContextAccessor contextAccessor, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _accountService = accountService;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordRequest { EmailAddress = string.Empty });
        }
        public IActionResult Login()
        {
            return View(new LoginUserRequest() { Email = string.Empty, Password = string.Empty });
        }
        public IActionResult Register()
        {
            return View(new RegisterUserRequest() { ConfirmPassword = string.Empty, Email = string.Empty, FullName = string.Empty, Password = string.Empty });
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
                ModelState.AddModelError("Login", "Cannot login");
                return View(request);
            }
            if (_contextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "ManagingPatient", new { area = "Admin" });
            }
            if (_contextAccessor.HttpContext.User.IsInRole("Patient"))
            {
                return RedirectToAction("Index", "Patient");
            }
            if (_contextAccessor.HttpContext.User.IsInRole("Doctor"))
            {
                return RedirectToAction("Index", "Doctor");
            }
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            if (!(await _accountService.RegisterAsync(request)))
            {
                return View(request);
            }
            return RedirectToAction("Login");
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
            var body = await _emailService.GetResetPasswordTemplate(user.UserName, url);
            BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMailAsync(request.EmailAddress, "Đặt lại mật khẩu", body));
            return View("ConfirmedEmail", request.EmailAddress);
        }

        public IActionResult ResetPassword(string code, string email)
        {
            if(code == null)
            {
                return NotFound("Cannot find the token");
            }
            return View(new ResetPasswordRequest { Code = code, Email = email });
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
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmedUrl = Url.Action("ConfirmedEmail","Account", new { code = code });
            var body = await _emailService.GetConfirmedEmailTemplate(user.UserName, confirmedUrl);
            BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMailAsync(email, "Xác thực email", body));
            return View("ConfirmedEmail", email);
        }
    }
}
