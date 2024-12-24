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
}
