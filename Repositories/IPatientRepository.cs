using AppointmentHospital.DTOs.Patient;

namespace AppointmentHospital.Repositories
{
    public interface IPatientRepository
    {
        Task<PatientResponse> GetPatientById(Guid userId);
        Task<PatientResponse> EditPatientInfo(Guid patientId ,PatientRequest request);  
    }
}
