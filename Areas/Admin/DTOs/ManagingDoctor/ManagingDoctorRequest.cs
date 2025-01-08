using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor
{
   public class ManagingDoctorRequest
{

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Only letters are allowed")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Specialization is required")]
    [Display(Name = "Specialization")]
    public Specialization Specializaiton { get; set; }

    [Required(ErrorMessage = "Experience years is required")]
    [Range(1, 50, ErrorMessage = "Experience must be between 1 and 50 years")]
    [Display(Name = "Years of Experience")]
    public int ExperienceYear { get; set; }


    [Required(ErrorMessage = "Gender is required")]
    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Invalid gender")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Degree is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Degree must be between 2 and 200 characters")]
    public string Degree { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [Display(Name = "Email Address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string EmailAddress { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }
}
}
