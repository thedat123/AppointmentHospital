using AppointmentHospital.DTOs.Account;
using AppointmentHospital.Models;
using static AppointmentHospital.DTOs.Account.AccountRequest;
namespace AppointmentHospital.Services
{
    public interface IAccountService
    {
        Task<AccountResponse> LoginAsync(LoginUserRequest request);
        Task<User> RegisterAsync(RegisterUserRequest request);
        Guid GetIdByEmail(string email);
    }
}
