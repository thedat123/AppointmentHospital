using AppointmentHospital.DTOs.Patient;
using AppointmentHospital.Migrations;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories
{
    public interface IPatientRepository
    {
        Task<PatientResponse> GetPatientById(Guid userId);
        Task<PatientResponse> EditPatientInfo(Guid patientId ,PatientRequest request);  
        void AddAcquaintance(Acquaintance acquaintance);
        List<DiagnosisHistory> GetDiagnosisHistoriesByPatientId(Guid patientId);
        List<DiagnosisHistory> GetDiagnosisHistoriesByAcquaintanceId(Guid acquaintanceId);
    }
}
