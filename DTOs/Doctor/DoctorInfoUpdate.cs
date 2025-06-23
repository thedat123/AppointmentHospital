using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.ViewModels
{
    public class DoctorInfoUpdate
    {
        public Guid DoctorId { get; set; }
        public string? FullName { get; set; }
        public string? Degree { get; set; }
        public int? SpecialityId { get; set; }
        public string? Introduction { get; set; }
        public string? OrganizationMember { get; set; }
        public string? Expertise { get; set; }
        public string? Awards { get; set; }
        public string? ResearchProject { get; set; }
        public string? TrainingProcess { get; set; }
        public string? WorkExperience { get; set; }
    }
}