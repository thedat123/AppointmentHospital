using AppointmentHospital.Models;
using Microsoft.Identity.Client;

namespace AppointmentHospital.Areas.Admin.DTOs.Statistic
{
    public class StatisticResponse
    {
        public int AppointmentAmount { set; get; }
        public int NewPatient { set; get; }
        public int OldPatient { set; get; }
        public TopDoctorStatistic TopDoctorStatistic { set; get; }
    }
    public class TopDoctorStatistic
    {
        public string DoctorName { set; get; }
        public string Specialization { set; get; }
        public int AppointmentAmount { set; get; }
    }
}
