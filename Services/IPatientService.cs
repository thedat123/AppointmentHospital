using AppointmentHospital.DTOs.Patient;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services
{
    public interface IPatientService
    {
        Task<PatientResponse> GetPatientById(Guid userId);
        Task<PatientResponse> EditPatientInfo(Guid patientId , PatientRequest request);
        void AddAcquaintance(Acquaintance acquaintance);
    }
}
