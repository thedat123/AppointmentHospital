using AppointmentHospital.Areas.Admin.DTOs.Appointment;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Services
{
    public interface IAppointmentService
    {
        Task<Pagination<AppointmentResponse>> GetAllAppointmentAsync(int page, string? searchTerm, int? specialityId, AppointmentStatus? status, string? appointmentDate);
        List<SelectListItem> GetSpecialization();
        List<SelectListItem> GetStatus();
        Task<AppointmentResponse> GetAppointmentAsync(Guid id);
    }
}
