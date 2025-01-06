using AppointmentHospital.DTOs.Account;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using static AppointmentHospital.DTOs.Account.AccountRequest;

namespace AppointmentHospital.Repositories.Implement
{
    public class AccountRepository : IAccountRepository
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _appDbContext;
        public AccountRepository(RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext appDbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _appDbContext = appDbContext;
        }
        public async Task<AccountResponse> LoginAsync(LoginUserRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Cannot find user");
            }
            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new AccountResponse { Success = false, Status = 403 };
            }
            if(!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AccountResponse { Success = false, Status = 400 };
            }
            var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            return new AccountResponse { Status = 200, Success = true };
        }

        public async Task<User> RegisterAsync(RegisterUserRequest request)
        {
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    UserName = request.Email,
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    throw new Exception("Cannot create new user");
                }
                var resultAddRole = await _userManager.AddToRoleAsync(user, "Patient");
                if (!resultAddRole.Succeeded)
                {
                    throw new Exception("Failed to assign role to user");
                }
                var patient = new Patient
                {
                    FullName = request.FullName,
                    Address = request.Address,
                    DateOfBirth = request.DateOfBirth,
                    User = user
                };
                await _appDbContext.Patients.AddAsync(patient);
                await _appDbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Guid GetIdByEmail(string email)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Email == email);
            return user.Id;
        }
    }
}
