using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentHospital.Models
{
    public class DiagnosisHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        public Guid? AcquaintanceId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public string Diagnosis { get; set; }
        public List<string> Prescription { get; set; } = new List<string>();
        public string DoctorNote { get; set; } = "";

        public DateTime DateTime { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

    }

}
