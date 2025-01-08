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
        Task AddFeedback(FeedbackRequest request);
        Task<FeedbackResponse> GetFeedback(Guid appointmentId);
        Task<bool> HasFeedback(Guid appointmentId);
        List<DiagnosisHistory> GetDiagnosisHistoriesByPatientId(Guid patientId);
        List<DiagnosisHistory> GetDiagnosisHistoriesByAcquaintanceId(Guid acquaintanceId);
    }
}
