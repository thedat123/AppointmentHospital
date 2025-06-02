using AppointmentHospital.Areas.Admin.DTOs.Appointment;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Pagination<AppointmentResponse>> GetAllAppointmentAsync(int page, string? searchTerm, int SpecialityId);
        List<SelectListItem> GetSpecialization();
        List<SelectListItem> GetStatus();
        Task<AppointmentResponse> GetAppointmentAsync(Guid id);
    }
}
