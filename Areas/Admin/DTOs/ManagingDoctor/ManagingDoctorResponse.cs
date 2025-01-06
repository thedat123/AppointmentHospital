
using AppointmentHospital.EnumStatus;

namespace AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor
{
    public class ManagingDoctorResponse
    {
        public Guid Id { get; set; }
        public string PhoneNumber { set;get; }
        public string FullName { get; set; }
        public string Specializaiton { get; set; }
        public string ImagePath { get; set; }
        public string EmailAddress { get;set ; }
        public string Gender { get; set; }
        public string Degree { get; set; }
        public string Description { get; set; }

        public int ExperienceYear { get; set; } 
    }
}
