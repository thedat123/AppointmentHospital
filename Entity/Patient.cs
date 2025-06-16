using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentHospital.Models
{
    public class Patient
    {
        [Key]
        public Guid PatientId { set;get; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string? Address  { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(20)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Identification number must be numeric.")]
        public string? PhoneNumber { get; set; }
        [ForeignKey("PatientId")]
        public virtual User User { set;get; }
        public bool IsDeleted { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        [MaxLength(50)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Identification number must be numeric.")]
        public string? IdentificationNumber { get; set; }
        public virtual ICollection<Appointment> Appointments { set; get; }

        public virtual ICollection<Acquaintance> Acquaintances { get; set; }
        public virtual ICollection<Feedback> Feedbacks { set; get; }
    }
}
