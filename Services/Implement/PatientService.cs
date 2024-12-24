using AppointmentHospital.DTOs.Patient;
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
    }
}
