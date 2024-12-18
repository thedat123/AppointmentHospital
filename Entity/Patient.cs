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
        [ForeignKey("PatientId")]
        public User User { set;get; }
        public ICollection<Appointment> Appointments { set; get; }

        public ICollection<Acquaintance> Acquaintances { get; set; }
    }
}
