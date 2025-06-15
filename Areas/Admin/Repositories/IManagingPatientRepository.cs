using AppointmentHospital.Areas.Admin.DTOs.AdminDashBoard;
using AppointmentHospital.Areas.Admin.DTOs.ManagingPatient;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Areas.Admin.Repositories
{
    public interface IManagingPatientRepository
    {
        Task<Pagination<ManagingPatientResponse>> GetAllPatientAsync(int page, string searchTerm, string statusFilter = null, string isDeletedFilter = null);
        Task<bool> CreateNewPatientAsync(ManagingPatientRequest request);
        Task<bool> SoftDeletePatientAsync(Guid id);
        Task EditPatientAsync(Guid id, ManagingPatientRequest request);
        Task<ManagingPatientResponse> GetPatientAsync(Guid id);
        Task<bool> BanPatientAsync(Guid id);
        Task<bool> UnbanPatientAsync(Guid id);
        Task<bool> RestorePatientAsync(Guid id);
    }
}
