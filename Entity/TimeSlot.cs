using AppointmentHospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Entity
{
    public class TimeSlot
    {
        [Key]
        public Guid TimeSlotId { get; set; }  

        [ForeignKey("DoctorId")]
        public Guid DoctorId { get; set; }  

        [Required]
        public DateTime StartTime { get; set; } 

        [Required]
        public DateTime EndTime { get; set; }  
        public virtual Doctor Doctor { get; set; }  
    }
}

