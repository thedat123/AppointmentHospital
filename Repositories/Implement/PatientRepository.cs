using AppointmentHospital.DTOs.Patient;
using AppointmentHospital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentHospital.Repositories.Implement
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext appDbContext;
        public PatientRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<PatientResponse> EditPatientInfo(Guid patientId, PatientRequest request)
        {
            try
            {
                var patient = await appDbContext.Patients.Include(p => p.User).Include(p => p.Acquaintances).Where(p => p.PatientId == patientId).FirstOrDefaultAsync();
                if (patient == null)
                {
                    throw new Exception("Cannot find user");
                }
                var user = patient.User;
                user.Email = request.EmailAddress;
                user.UserName = request.UserName;
                appDbContext.Update(user);
                patient.FullName = request.FullName;
                patient.Address = request.Address;
                patient.DateOfBirth = request.DateOfBirth;
                appDbContext.Update(patient);
                await appDbContext.SaveChangesAsync();
                var patientResponse = new PatientResponse
                {
                    UserName = patient.User.UserName,
                    EmailAddress = patient.User.Email,
                    FullName = patient.FullName,
                    DateOfBirth = patient.DateOfBirth,
                    Address = patient.Address,
                    PatientId = patient.PatientId,
                    Acquaintance = patient.Acquaintances.Select(a => new AcquaintanceResponse
                    {
                        Id = a.Id,
                        Gender = a.Gender,
                        IdentificationNumber = a.IdentificationNumber,
                        Address = a.Address,
                        DateOfBirth = a.DateOfBirth,
                        Name = a.Name
                    }).ToList()
                };
                return patientResponse;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<PatientResponse> GetPatientById(Guid userId)
        {
            var patient = await appDbContext.Patients.Include(p => p.Acquaintances)
                                                     .Include(p => p.User)
                                                     .Where(p => p.PatientId == userId)
                                                     .FirstOrDefaultAsync();
            var patientResponse = new PatientResponse
            {
                UserName = patient.User.UserName,
                EmailAddress = patient.User.Email,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address,
                PatientId = patient.PatientId,
                Acquaintance = patient.Acquaintances.Select(a => new AcquaintanceResponse
                {
                    Id = a.Id,
                    Gender = a.Gender,
                    IdentificationNumber = a.IdentificationNumber,
                    Address = a.Address,
                    DateOfBirth = a.DateOfBirth,
                    Name = a.Name
                }).ToList()
            };
            return patientResponse;
        }

        public void AddAcquaintance(Acquaintance acquaintance)
        {
            appDbContext.Add(acquaintance);
            appDbContext.SaveChanges();
        }

        public List<DiagnosisHistory> GetDiagnosisHistoriesByPatientId(Guid patientId)
        {
            return appDbContext.DiagnosisHistory.Where(d => d.PatientId == patientId).ToList();
        }

        public List<DiagnosisHistory> GetDiagnosisHistoriesByAcquaintanceId(Guid acquaintanceId)
        {
            return appDbContext.DiagnosisHistory.Where(d => d.AcquaintanceId == acquaintanceId).ToList();
        }
    }
}
