using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.Appointment
{
    public class AppointmentResponse
    {
        public Guid AppointmentId { get; set; }

        public string PatientName { get; set; }

        public Guid PatientId { get; set; } 
        public string DoctorName { get; set; }

        public Guid DoctorId { get; set; } 

        public string Specialization { get; set; }

        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; } 

   
        public string? CancellationReason { get; set; }

        public string? Notes { get; set; }

      
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
