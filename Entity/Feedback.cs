using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentHospital.Models
{
    public class Feedback
    {
        [Key]
        public Guid FeedbackId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [Range(1, 5)]
        public int ProfessionalSkills { get; set; }

        [Required]
        [Range(1, 5)]
        public int Communication { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("AppointmentId")]
        public Guid? AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("Doctor")]
        public Guid? DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }
        [ForeignKey("PatientId")]
        public Guid? PatientId { get; set; }
        public virtual Patient Patient { get; set; }
    }
}