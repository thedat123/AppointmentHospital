using AppointmentHospital.Areas.Admin.DTOs.Appointment;
using AppointmentHospital.Areas.Admin.Repositories;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Services.Implement
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }
        public async Task<Pagination<AppointmentResponse>> GetAllAppointmentAsync(int page, string? searchTerm, int? specialityId, AppointmentStatus? status, string? appointmentDate)
        {
            return await _appointmentRepository.GetAllAppointmentAsync(page, searchTerm, specialityId, status, appointmentDate);
        }

        public async Task<AppointmentResponse> GetAppointmentAsync(Guid id)
        {
            return await _appointmentRepository.GetAppointmentAsync(id);
        }

        public List<SelectListItem> GetSpecialization()
        {
            return _appointmentRepository.GetSpecialization();
        }

        public List<SelectListItem> GetStatus()
        {
            return _appointmentRepository.GetStatus();
        }
    }
}
