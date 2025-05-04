using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;

namespace FinalProject.Entity
{
    public class Specialities
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SpecialityName { get; set; } = "";

        public string SpecialityImage { get; set; } = "";
    }
}