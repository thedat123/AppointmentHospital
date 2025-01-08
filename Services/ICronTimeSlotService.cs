using System;

namespace AppointmentHospital.Services;

public interface ICronTimeSlotService
{
    Task DeleteOldSchedules();
}
