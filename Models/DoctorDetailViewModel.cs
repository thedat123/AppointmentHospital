using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Models
{
    public class DoctorDetailViewModel
    {
        public Doctor Doctor { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
    }
}
