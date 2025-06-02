using AppointmentHospital.Areas.Admin.DTOs.AdminDashBoard;
using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Services
{
    public interface IManagingDoctorService
    {
        Task<Pagination<ManagingDoctorResponse>> GetAllDoctor(int page, string searchTerm, int specialityId);
        List<SelectListItem> GetSpecialization();
        Task CreateNewDoctorAsync(ManagingDoctorRequest request);
        Task DeleteDoctorAsync(Guid id);
        Task<ManagingDoctorResponse> GetDoctorAsync(Guid id);
        Task EditDoctorAsync(Guid id, ManagingDoctorRequest request);
    }
}
