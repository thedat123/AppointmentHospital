using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Models
{
    public class Drug
    {
        [Key]
        public Guid DrugId { get; set; }

        [Required]
        public string DrugName { get; set; }
    }
}

