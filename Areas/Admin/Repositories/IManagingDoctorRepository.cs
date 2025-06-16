using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentHospital.Areas.Admin.Repositories
{
    public interface IManagingDoctorRepository
    {
        Task<Pagination<ManagingDoctorResponse>> GetAllDoctor(int page, string searchTerm, int specialityId);
        Task<bool> GetTimeSlotByDoctorIdAsync(Guid doctorId);
        List<SelectListItem> GetSpecialization();
        Task CreateNewDoctorAsync(ManagingDoctorRequest request);
        Task DeleteDoctorAsync(Guid id);
        Task<ManagingDoctorResponse> GetDoctorAsync(Guid id);
        Task EditDoctorAsync(Guid id, ManagingDoctorRequest request);
    }
}
