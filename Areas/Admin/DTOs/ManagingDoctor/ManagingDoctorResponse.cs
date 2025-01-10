using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor
{
    public class ManagingDoctorResponse
    {
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters")]
    public string PhoneNumber { set; get; }

    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Full name can only contain letters and spaces")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Specialization is required")]
    [Display(Name = "Specialization")]
    public string Specializaiton { get; set; }

    [Display(Name = "Profile Image")]
    public string ImagePath { get; set; }

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [Display(Name = "Email Address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string EmailAddress { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Invalid gender selection")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Degree is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Degree must be between 2 and 200 characters")]
    public string Degree { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Experience years is required")]
    [Range(1, 50, ErrorMessage = "Experience must be between 1 and 50 years")]
    [Display(Name = "Years of Experience")]
    public int ExperienceYear { get; set; }
    }
}
