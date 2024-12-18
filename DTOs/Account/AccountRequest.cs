using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.DTOs.Account
{
    public class AccountRequest
    {
        public class LoginUserRequest
        {

            [Required(ErrorMessage = "Email không được để trống.")]
            [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")]
            public string Password { get; set; }
        }
        public class RegisterUserRequest
        {
            [Required(ErrorMessage = "Email không được để trống.")]
            [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
            public string Email { set; get; }

            [Required(ErrorMessage = "Họ và tên không được để trống.")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
            public string FullName { set; get; }

            [Required(ErrorMessage = "Mật khẩu không được để trống.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
            public string Password { set; get; }

            [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
            [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp với mật khẩu.")]
            public string ConfirmPassword { set; get; }
        }
        public class ForgetPasswordRequest
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string EmailAddress { get; set; }
        }
        public class UnconfirmedEmailRequest
        {
            public string Email { set; get; }
        }
        public class ResetPasswordRequest
        {
            public string Email { set; get; }
            public string Password { set; get; }
            public string ConfirmPassword { set; get; }
            public string Code { set; get; }
        }
    }
}
