using System;
using AppointmentHospital.Entity;

namespace AppointmentHospital.Services;

public interface ITimeSlotService
{
    public List<TimeSlot> GetTimeSlotByDoctorId(Guid doctorId);
}
