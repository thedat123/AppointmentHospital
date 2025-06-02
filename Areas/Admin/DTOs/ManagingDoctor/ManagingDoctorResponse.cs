using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor
{
    public class ManagingDoctorResponse
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public string FullName { get; set; }


        [Required(ErrorMessage = "Speciality Id is required")]
        [Display(Name = "Speciality Id")]
        public int SpecialityId { get; set; }

        [Display(Name = "Speciality Name")]
        public string SpecialityName { get; set; }

        [Display(Name = "Profile Image")]
        public string ImagePath { get; set; }

        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
        public string EmailAddress { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Degree is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Degree must be between 2 and 200 characters")]
        public string Degree { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; }

        [Display(Name = "Years of Experience")]
        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
        public int ExperienceYear { get; set; }

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