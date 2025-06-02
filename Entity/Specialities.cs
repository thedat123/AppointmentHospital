using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;

namespace AppointmentHospital.Models
{
    public class Specialities
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SpecialityName { get; set; } = "";

        public string SpecialityImage { get; set; } = "";

        public string? Introduction { get; set; } = "";
        public string? Expertise { get; set; } = "";
        public string? Treatment { get; set; } = "";
        public string? Equipment { get; set; } = "";

        public string? Aminities { get; set; } = "";

        public string? Mission { get; set; } = "";

        [ValidateNever]
        public virtual ICollection<Doctor>? Doctor { get; set; }

        public string? Service { get; set; } = "";

        public string? Archivement { get; set; } = "";
    }
}