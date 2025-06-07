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
        public async Task<Pagination<ManagingDoctorResponse>> GetAllDoctor(int page, string searchTerm, int specialityId)
        {
            // Include các quan hệ cần thiết
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialities)
                .AsQueryable();

            // Tìm theo tên gần đúng (chứa, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedTerm = searchTerm.Trim().ToLower();
                query = query.Where(d => d.FullName.ToLower().Contains(normalizedTerm));
            }

            // Lọc theo chuyên khoa nếu có chọn
            if (specialityId != 0)
            {
                query = query.Where(d => d.SpecialityId == specialityId);
            }

            // Phân trang dữ liệu
            var paginatedList = await Pagination<Doctor>.PaginatedList(query, page);

            // Mapping sang response
            var doctorList = paginatedList.Select(d => new ManagingDoctorResponse
            {
                EmailAddress = d.User?.Email,
                FullName = d.FullName,
                Degree = d.Degree,
                Id = d.DoctorId,
                SpecialityId = d.SpecialityId ?? 0,
                SpecialityName = d.Specialities?.SpecialityName,
                ImagePath = d.ImagePath,
                Introduction = d.Introduction,
                Awards = d.Awards,
                Expertise = d.Expertise,
                OrganizationMember = d.OrganizationMember,
                ResearchProject = d.ResearchProject,
                TrainingProcess = d.TrainingProcess,
                WorkExperience = d.WorkExperience
            }).ToList();

            return new Pagination<ManagingDoctorResponse>(doctorList, page, paginatedList.TotalItems);
        }


        public List<SelectListItem> GetSpecialization()
        {
            var selectListItem = _context.Specialities.Select(s => new SelectListItem
            {
                Text = s.SpecialityName,
                Value = s.Id.ToString(),
            }).ToList();

            return selectListItem;
        }

        public async Task CreateNewDoctorAsync(ManagingDoctorRequest request)
        {
            var user = new User
            {
                Email = request.EmailAddress,
                UserName = request.EmailAddress,
                EmailConfirmed = true
            };
            var doctor = new Doctor
            {
                SpecialityId = request.SpecialityId,
                FullName = request.FullName,
                Degree = request.Degree,
                ImagePath = request.ImagePath,
                Introduction = request.Introduction,
                Awards = request.Awards,
                Expertise = request.Expertise,
                OrganizationMember = request.OrganizationMember,
                ResearchProject = request.ResearchProject,
                TrainingProcess = request.TrainingProcess,
                WorkExperience = request.WorkExperience,
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
                Degree = d.Degree,
                ImagePath = d.ImagePath,
                SpecialityId = d.SpecialityId ?? 0,
                Id = d.DoctorId,
                Introduction = d.Introduction,
                Awards = d.Awards,
                Expertise = d.Expertise,
                OrganizationMember = d.OrganizationMember,
                ResearchProject = d.ResearchProject,
                TrainingProcess = d.TrainingProcess,
                WorkExperience = d.WorkExperience,
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
            doctor.SpecialityId = request.SpecialityId;
            doctor.Degree = request.Degree;
            doctor.ImagePath = request.ImagePath;
            doctor.Introduction = request.Introduction;
            doctor.Awards = request.Awards;
            doctor.Expertise = request.Expertise;
            doctor.OrganizationMember = request.OrganizationMember;
            doctor.ResearchProject = request.ResearchProject;
            doctor.TrainingProcess = request.TrainingProcess;
            doctor.WorkExperience = request.WorkExperience;
            _context.Update(doctor);
            await _context.SaveChangesAsync();
        }
    }
}
