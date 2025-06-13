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
                // Trả về lỗi rõ ràng thay vì throw exception
                return new AccountResponse
                {
                    Success = false,
                    Status = 404,
                    Message = "Cannot find user"
                };
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new AccountResponse
                {
                    Success = false,
                    Status = 403,
                    Message = "Email not confirmed"
                };
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AccountResponse
                {
                    Success = false,
                    Status = 400,
                    Message = "Incorrect password"
                };
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            
            return new AccountResponse
            {
                Success = true,
                Status = 200,
                Message = "Login successful"
            };
        }

        public async Task<(User User, AccountResponse Response)> RegisterAsync(RegisterUserRequest request)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return (null, new AccountResponse
                {
                    Success = false,
                    Status = 400,
                    Message = "User with this email already exists"
                });
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                // Combine error messages from IdentityResult
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                return (null, new AccountResponse
                {
                    Success = false,
                    Status = 400,
                    Message = $"Failed to create user: {errorMessage}"
                });
            }

            var resultAddRole = await _userManager.AddToRoleAsync(user, "Patient");
            if (!resultAddRole.Succeeded)
            {
                // Combine error messages from IdentityResult
                var errorMessage = string.Join("; ", resultAddRole.Errors.Select(e => e.Description));
                return (null, new AccountResponse
                {
                    Success = false,
                    Status = 400,
                    Message = $"Failed to assign role to user: {errorMessage}"
                });
            }

            var patient = new Patient
            {
                FullName = request.FullName,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                User = user
            };

            try
            {
                await _appDbContext.Patients.AddAsync(patient);
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (null, new AccountResponse
                {
                    Success = false,
                    Status = 500,
                    Message = $"Failed to save patient data: {ex.Message}"
                });
            }

            return (user, new AccountResponse
            {
                Success = true,
                Status = 200,
                Message = "Registration successful"
            });
        }

        public Guid GetIdByEmail(string email)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Email == email);
            return user.Id;
        }
    }
}
