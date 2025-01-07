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
            catch (Exception ex)
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
        public async Task AddFeedback(FeedbackRequest request)
        {
            try
            {
                var appointment = await appDbContext.Appointments.Where(a => a.AppointmentId == request.AppointmentId).FirstOrDefaultAsync();
                if (appointment == null)
                {
                    throw new Exception("Cannot find appointment");
                }
                var feedback = new Feedback
                {
                    AppointmentId = request.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    Communication = request.Communication,
                    ProfessionalSkills = request.ProfessionalSkills,
                    CreatedAt = DateTime.Now
                };
                await appDbContext.AddAsync(feedback);
                await appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task<FeedbackResponse> GetFeedback(Guid appointmentId) {
            var feedback = await appDbContext.Feedbacks.Where(f => f.AppointmentId == appointmentId).Select(f => new FeedbackResponse
            {
                AppointmentId = f.AppointmentId,
                Comment = f.Comment,
                Communication = f.Communication,
                Rating = f.Rating,
                ProfessionalSkills = f.ProfessionalSkills,
                CreatedAt = f.CreatedAt,
                DoctorName = f.Doctor.FullName,
                DoctorSpecialization = EnumExtensions.GetDisplayName(f.Doctor.Specializaiton),
            }).FirstOrDefaultAsync();
            return feedback;
                                          
        }
    }
}
