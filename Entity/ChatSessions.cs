using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentHospital.Models;

namespace AppointmentHospital.Models
{
    public class ChatSessions
    {
        [Key]
        public Guid SessionId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public string SessionName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}