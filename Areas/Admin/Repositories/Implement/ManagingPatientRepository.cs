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
            if(createResult.Succeeded)
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

        public async Task DeletePatientAsync(Guid id)
        {
            var patient = await _context.Patients.Include(p => p.User)
                                                 .Include(p => p.Appointments)
                                                 .FirstOrDefaultAsync(p => p.PatientId == id);
            _context.Appointments.RemoveRange(patient.Appointments);
            _context.Patients.Remove(patient);
            _context.Users.Remove(patient.User);
           
            await _context.SaveChangesAsync();
        }

        public async Task EditPatientAsync(Guid id, ManagingPatientRequest request)
        {
            var patient = await _context.Patients.Include(p => p.User).Where(p => p.PatientId == id).FirstOrDefaultAsync();
            var userPatient = patient.User;
            userPatient.Email = request.EmailAddress;
            userPatient.PhoneNumber = request.PhoneNumber;
            _context.Update(userPatient);
            patient.DateOfBirth = request.DateOfBirth;
            patient.FullName = request.FullName;
            patient.Address = request.Address;
            _context.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task<ManagingPatientResponse> GetPatientAsync(Guid id)
        {
            var patient = await _context.Patients.Where(p => p.PatientId == id).Select(p => new ManagingPatientResponse
            {
                Address = p.Address,
                EmailAddress = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
            }).FirstOrDefaultAsync();
            return patient;
        }
        public async Task<Pagination<ManagingPatientResponse>> GetAllPatientAsync(int page, string searchTerm)
        {
            var query =  _context.Patients.Select(p => new ManagingPatientResponse
            {
                Id = p.PatientId,
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                FullName = p.FullName,
                EmailAddress = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
            });
            if(!searchTerm.IsNullOrEmpty())
            {
                query = query.Where(p => p.FullName.ToLower().Contains(searchTerm.ToLower()));
            }
            var patientResponse = await Pagination<ManagingPatientResponse>.PaginatedList(query, page);
            return patientResponse;
        }
    }
}
