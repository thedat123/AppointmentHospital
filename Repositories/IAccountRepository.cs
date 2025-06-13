using AppointmentHospital.DTOs.Account;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.Identity.Data;
using static AppointmentHospital.DTOs.Account.AccountRequest;

namespace AppointmentHospital.Repositories
{
    public interface IAccountRepository
    {
        Task<AccountResponse> LoginAsync(LoginUserRequest request);
        Task<(User User, AccountResponse Response)> RegisterAsync(RegisterUserRequest request);
        Guid GetIdByEmail(string email);
    }
}
