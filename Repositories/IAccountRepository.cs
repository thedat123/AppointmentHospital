using AppointmentHospital.Models;
using Microsoft.AspNetCore.Identity.Data;
using static AppointmentHospital.DTOs.Account.AccountRequest;

namespace AppointmentHospital.Repositories
{
    public interface IAccountRepository
    {
        Task<bool> LoginAsync(LoginUserRequest request);
        Task<User> RegisterAsync(RegisterUserRequest request);
    }
}
