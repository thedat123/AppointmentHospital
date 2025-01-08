using AppointmentHospital.Models;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.DTOs.Patient
{
    public class PatientResponse
    {
        public Guid PatientId { set; get; }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public List<AcquaintanceResponse> Acquaintance { get; set; }
    }
    public class AcquaintanceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int IdentificationNumber { get; set; }
        public string Address { get; set; }
    }
        public class FeedbackResponse
    {
        public Guid? AppointmentId { get; set; }
        public int Rating { get; set; }
        public int ProfessionalSkills { get; set; }
        public int Communication { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DoctorName { get; set; }
        public string PatientName {get;set;}
        public string DoctorSpecialization { get; set; }
    }
}
