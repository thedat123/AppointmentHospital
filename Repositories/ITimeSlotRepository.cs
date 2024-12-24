using System;
using AppointmentHospital.Entity;

namespace AppointmentHospital.Repositories;

public interface ITimeSlotRepository
{
    public List<TimeSlot> GetTimeSlotByDoctorId(Guid doctorId);
}
