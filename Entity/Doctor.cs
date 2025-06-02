using AppointmentHospital.Controllers;
using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentHospital.Models
{
    public class Doctor
    {
        [Key]
        public Guid DoctorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        public int? SpecialityId { get; set; }
        public string? ImagePath { get; set; }
        public string? Degree { get; set; }
        public string? Introduction { get; set; }
        public string? OrganizationMember { get; set; }
        public string? Expertise { get; set; }
        public string? Awards { get; set; }
        public string? ResearchProject { get; set; }
        public string? TrainingProcess { get; set; }
        public string? WorkExperience { get; set; }

        [ForeignKey("DoctorId")]       
        public virtual User User { get; set; }

        [ForeignKey("SpecialityId")]
        public virtual Specialities Specialities { get; set; }  

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<TimeSlot> TimeSlots { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
