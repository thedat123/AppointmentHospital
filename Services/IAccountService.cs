using AppointmentHospital.Models;
using static AppointmentHospital.DTOs.Account.AccountRequest;
namespace AppointmentHospital.Services
{
    public interface IAccountService
    {
        Task<bool> LoginAsync(LoginUserRequest request);
        Task<User> RegisterAsync(RegisterUserRequest request);
    }
}
