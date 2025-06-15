using AppointmentHospital.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.Areas.Admin.DTOs.AdminDashBoard
{
    public class ManagingPatientResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }

        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsBanned { get; set; }
    }
}
