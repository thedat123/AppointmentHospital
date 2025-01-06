using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.ViewModels
{
    public class DoctorRequest
    {
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Specialization Specializaiton { get; set; }

        [Required]
        [Range(0, 50)]
        [Display(Name = "Years of Experience")]
        public int ExperienceYear { get; set; }

        [Required]
        public string Degree { get; set; }

        [Required]
        public string Description { get; set; }

    }
}