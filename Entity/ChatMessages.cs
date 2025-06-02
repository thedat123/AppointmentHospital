using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentHospital.Models
{
    public class ChatMessages
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        public bool IsFromPatient { get; set; }

        public string MessageText { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}