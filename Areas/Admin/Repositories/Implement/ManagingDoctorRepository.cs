using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppointmentHospital.Areas.Admin.Repositories.Implement
{
    public class ManagingDoctorRepository : IManagingDoctorRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public ManagingDoctorRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<Pagination<ManagingDoctorResponse>> GetAllDoctor(int page, string searchTerm, Specialization? specialization)
        {
            var query = _context.Doctors.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm) && specialization == null)
            {
                query = query.Where(d => d.FullName.ToLower() == searchTerm.ToLower());
            }
            if (string.IsNullOrEmpty(searchTerm) && specialization != null)
            {
                query = query.Where(d => d.Specializaiton == specialization);
            }
            if (!string.IsNullOrEmpty(searchTerm) && specialization != null)
            {
                query = query.Where(d => d.FullName.ToLower() == searchTerm.ToLower() && d.Specializaiton == specialization);
            }
            var paginatedList = await Pagination<Doctor>.PaginatedList(query, page);
            var doctorList = paginatedList.Select(d => new ManagingDoctorResponse {
                EmailAddress = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                FullName = d.FullName,
                Gender = d.Gender,
                Degree = d.Degree,
                Id = d.DoctorId,
                ExperienceYear = d.ExperienceYear,
                Specializaiton = EnumExtensions.GetDisplayName(d.Specializaiton)
            }).ToList();
            return new Pagination<ManagingDoctorResponse> (doctorList, page, paginatedList.TotalItems);
        }

        public List<SelectListItem> GetSpecialization()
        {
            var selectListItem = Enum.GetValues(typeof(Specialization)).Cast<Specialization>().Select(s => new SelectListItem
            {
                Text = GetDisplayNameEnum(s),
                Value = ((int)s).ToString()
            }).ToList();
            return selectListItem;
        }
        public static string GetDisplayNameEnum(Enum value)
        {
            var displayName =  value.GetType().GetField(value.ToString()).GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
            return displayName;
        }

        public async Task CreateNewDoctorAsync(ManagingDoctorRequest request)
        {
            var user = new User
            {
                Email = request.EmailAddress,
                UserName = request.EmailAddress,
                EmailConfirmed = true,
                PhoneNumber = request.PhoneNumber
            };
            var doctor = new Doctor
            {
                Specializaiton = request.Specializaiton,
                ExperienceYear = request.ExperienceYear,
                FullName = request.FullName,
                User = user
            };
            await _userManager.CreateAsync(user, "Doctor123#");
            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(Guid id)
        {
            var doctor = await _context.Doctors.Include(d => d.User).Include(d => d.Appointments).Where(d => d.DoctorId == id).FirstOrDefaultAsync();
            _context.Appointments.RemoveRange(doctor.Appointments);
            _context.Doctors.Remove(doctor);
            _context.Users.Remove(doctor.User);
            await _context.SaveChangesAsync();
        }

        public async Task<ManagingDoctorResponse> GetDoctorAsync(Guid id)
        {
            var doctor = await _context.Doctors.Include(d => d.User).Where(d => d.DoctorId == id).Select(d => new ManagingDoctorResponse
            {
                FullName = d.FullName,
                EmailAddress = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                Degree = d.Degree,
                Gender = d.Gender,
                Description = d.Description,
                ImagePath = d.ImagePath,
                Specializaiton = EnumExtensions.GetDisplayName(d.Specializaiton),
                ExperienceYear = d.ExperienceYear
            }).FirstOrDefaultAsync();
            return doctor;
        }

        public async Task EditDoctorAsync(Guid id, ManagingDoctorRequest request)
        {
            var doctor = await _context.Doctors.Include(d => d.User).Where(d => d.DoctorId == id).FirstOrDefaultAsync();
            doctor.FullName = request.FullName;
            doctor.User.UserName = request.EmailAddress;
            doctor.User.NormalizedEmail = request.EmailAddress.ToUpper();
            doctor.User.Email = request.EmailAddress;
            doctor.User.PhoneNumber = request.PhoneNumber;
            doctor.ExperienceYear = request.ExperienceYear;
            doctor.Specializaiton = request.Specializaiton;
            _context.Update(doctor);
            await _context.SaveChangesAsync();
        }
    }
}
