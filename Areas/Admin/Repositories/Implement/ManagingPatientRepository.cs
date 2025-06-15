using AppointmentHospital.Areas.Admin.DTOs.AdminDashBoard;
using AppointmentHospital.Areas.Admin.DTOs.ManagingPatient;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentHospital.Areas.Admin.Repositories.Implement
{
    public class ManagingPatientRepository : IManagingPatientRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ManagingPatientRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> CreateNewPatientAsync(ManagingPatientRequest request)
        {
            var user = new User
            {
                Email = request.EmailAddress,
                UserName = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
            };
            var createResult = await _userManager.CreateAsync(user, "Patient123#");
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");
                var patient = new Patient
                {
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    User = user
                };
                await _context.AddAsync(patient);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SoftDeletePatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return false;
            }

            patient.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task EditPatientAsync(Guid id, ManagingPatientRequest request)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            var userPatient = patient.User;
            userPatient.Email = request.EmailAddress;
            userPatient.PhoneNumber = request.PhoneNumber;
            _context.Update(userPatient);
            patient.DateOfBirth = request.DateOfBirth;
            patient.FullName = request.FullName;
            patient.Address = request.Address;
            patient.IsBanned = false;
            patient.IsDeleted = false;
            _context.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task<ManagingPatientResponse> GetPatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.PatientId == id && !p.IsBanned && !p.IsDeleted)
                .Select(p => new ManagingPatientResponse
                {
                    Address = p.Address,
                    EmailAddress = p.User.Email,
                    PhoneNumber = p.PhoneNumber,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Id = p.PatientId
                })
                .FirstOrDefaultAsync();
            return patient;
        }

        public async Task<Pagination<ManagingPatientResponse>> GetAllPatientAsync(int page, string searchTerm, string statusFilter = null, string isDeletedFilter = null)
        {
            var query = _context.Patients
                .Include(p => p.User)
                .Select(p => new ManagingPatientResponse
                {
                    Id = p.PatientId,
                    DateOfBirth = p.DateOfBirth,
                    Address = p.Address,
                    FullName = p.FullName,
                    EmailAddress = p.User.Email,
                    PhoneNumber = p.PhoneNumber,
                    IsBanned = p.IsBanned,
                    IsDeleted = p.IsDeleted
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.FullName.ToLower().Contains(searchTerm.ToLower()));
            }

            if (statusFilter == "banned")
            {
                query = query.Where(p => p.IsBanned);
            }
            else
            {
                query = query.Where(p => !p.IsBanned); // Default to non-banned patients
            }

            if (isDeletedFilter == "deleted")
            {
                query = query.Where(p => p.IsDeleted);
            }
            else
            {
                query = query.Where(p => !p.IsDeleted); // Default to non-deleted patients
            }

            var patientResponse = await Pagination<ManagingPatientResponse>.PaginatedList(query, page);
            return patientResponse;
        }

        public async Task<bool> BanPatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return false;
            }

            patient.IsBanned = !patient.IsBanned;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnbanPatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return false;
            }

            patient.IsBanned = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestorePatientAsync(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return false;
            }

            patient.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
