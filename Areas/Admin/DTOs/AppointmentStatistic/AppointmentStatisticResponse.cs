using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;

namespace AppointmentHospital.Areas.Admin.DTOs.AppointmentStatistic
{
    public class AppointmentStatisticResponse
    {
        public int TotalAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }

        public int NewPatients { get; set; }
        public int ReturnedPatients { get; set; }

        public List<DoctorResponse> Top5DoctorWithMostAppointments { get; set; }
    }
    public class DoctorResponse
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; }
        public Specialities Specialities { get; set; }
        public int TotalConfirmAppointments { get; set; }
        public int ExperienceYear { get; set; }
    }
}
