using AppointmentHospital.Areas.Admin.DTOs.Appointment;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppointmentHospital.Areas.Admin.Repositories.Implement
{
   public class AppointmentRepository : IAppointmentRepository
   {
       private readonly AppDbContext _context;
       public AppointmentRepository(AppDbContext context)
       {
           _context = context;
       }
       public async Task<Pagination<AppointmentResponse>> GetAllAppointmentAsync(int page, string? searchTerm, int SpecialityId)
       {
           var query = _context.Appointments.AsQueryable();
           if (searchTerm != null)
           {
               query = query.Where(a => a.Doctor.FullName.ToLower().Contains(searchTerm.ToLower()) || a.Patient.FullName.ToLower().Contains(searchTerm.ToLower()));
           }
           if (searchTerm != null)
           {
               query = query.Where(a => (a.Patient.FullName.ToLower().Contains(searchTerm.ToLower()) || a.Doctor.FullName.ToLower().Contains(searchTerm.ToLower())) && a.Doctor.SpecialityId == SpecialityId);
           }
           var paginatedList = await Pagination<Appointment>.PaginatedList(query, page);
           var appointmentList = paginatedList.Select(a => new AppointmentResponse {
                AppointmentId = a.AppointmentId,
                AppointmentTime = a.AppointmentTime,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.FullName,
                Status = a.Status,
                PatientId = a.AcquaintanceId.HasValue ? _context.Acquaintances.Where(ac => ac.Id == a.AcquaintanceId).FirstOrDefault().Id : a.PatientId,
                PatientName = a.AcquaintanceId.HasValue ? _context.Acquaintances.Where(ac => ac.Id == a.AcquaintanceId).FirstOrDefault().Name : a.Patient.FullName,
                CreatedAt = a.CreatedAt,
                SpecialityId = a.Doctor.SpecialityId ?? 0,
           }).ToList();
           return new Pagination<AppointmentResponse>(appointmentList, page, paginatedList.TotalItems);
       }

       public List<SelectListItem> GetStatus()
       {
           var statusList = Enum.GetValues(typeof(AppointmentStatus)).Cast<AppointmentStatus>().Select(s => new SelectListItem
           {
               Text = s.ToString(),
               Value = ((int)s).ToString()
           }).ToList();
           return statusList;
       }

       public List<SelectListItem> GetSpecialization()
       {
           var specializationList = _context.Specialities.Select(s => new SelectListItem
           {
               Text = s.SpecialityName,
               Value = s.Id.ToString()
           }).ToList();
           return specializationList;
       }
       public async Task<AppointmentResponse> GetAppointmentAsync(Guid id)
       {
           var appointment = await _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor).Include(a => a.Acquaintance).Select(a => new AppointmentResponse
           {
               PatientId = a.AcquaintanceId.HasValue ? a.Acquaintance.Id : a.Patient.PatientId,
               PatientName = a.AcquaintanceId.HasValue ? a.Acquaintance.Name : a.Patient.FullName,
               DoctorId = a.DoctorId,
               SpecialityId = a.Doctor.SpecialityId,
               SpecialityName = a.Doctor.Specialities.SpecialityName,
               AppointmentId = a.AppointmentId,
               DoctorName = a.Doctor.FullName,
               Status = a.Status,
               AppointmentTime = a.AppointmentTime,
               CancellationReason = a.CancellationReason,
               Notes = a.Notes,
           }).FirstOrDefaultAsync();
           return appointment;
       }
   }
}
