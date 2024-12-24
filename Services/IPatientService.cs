using AppointmentHospital.DTOs.Patient;

namespace AppointmentHospital.Services
{
    public interface IPatientService
    {
        Task<PatientResponse> GetPatientById(Guid userId);
        Task<PatientResponse> EditPatientInfo(Guid patientId , PatientRequest request);
    }
}
