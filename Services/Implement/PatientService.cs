using AppointmentHospital.DTOs.Patient;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientResponse> EditPatientInfo(Guid patientId ,PatientRequest request)
        {
            return await _patientRepository.EditPatientInfo(patientId,request);
        }

        public async Task<PatientResponse> GetPatientById(Guid userId)
        {
            return await _patientRepository.GetPatientById(userId);
        }

        public void AddAcquaintance(Acquaintance acquaintance)
        {
            _patientRepository.AddAcquaintance(acquaintance);
        }
        public async Task AddFeedback(FeedbackRequest request) {
            await _patientRepository.AddFeedback(request);
        }
        public async Task<FeedbackResponse> GetFeedback(Guid appointmentId){
           return await _patientRepository.GetFeedback(appointmentId);
        }
        public async Task<bool> HasFeedback(Guid appointmentId) {
            return await _patientRepository.HasFeedback(appointmentId);
        }

        public List<DiagnosisHistory> GetDiagnosisHistoriesByPatientId(Guid patientId){
            return _patientRepository.GetDiagnosisHistoriesByPatientId(patientId);
        }

        public List<DiagnosisHistory> GetDiagnosisHistoriesByAcquaintanceId(Guid acquaintanceId){
            return _patientRepository.GetDiagnosisHistoriesByAcquaintanceId(acquaintanceId);
        }
    }
}
