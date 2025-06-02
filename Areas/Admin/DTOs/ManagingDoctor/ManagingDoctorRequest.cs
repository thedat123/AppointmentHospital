using AppointmentHospital.EnumStatus;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor
{
   public class ManagingDoctorRequest
{

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Speciality Id is required")]
    [Display(Name = "SpecialityId")]
    public int SpecialityId { get; set; }

    [Required(ErrorMessage = "Degree is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Degree must be between 2 and 200 characters")]
    public string Degree { get; set; }

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [Display(Name = "Email Address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string EmailAddress { get; set; }

    [Display(Name = "Image Path")]
    public string? ImagePath { get; set; }

    [Display(Name = "Introduction")]
    public string? Introduction { get; set; }

    [Display(Name = "Awards")]
    public string? Awards { get; set; }

    [Display(Name = "Expertise")]
    public string? Expertise { get; set; }

    [Display(Name = "Organization Member")]
    public string? OrganizationMember { get; set; }

    [Display(Name = "Research Project")]
    public string? ResearchProject { get; set; }

    [Display(Name = "Training Process")]
    public string? TrainingProcess { get; set; }

    [Display(Name = "Work Experience")]
    public string? WorkExperience { get; set; }
}
}
