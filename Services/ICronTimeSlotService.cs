using System;

namespace AppointmentHospital.Services;

public interface ICronTimeSlotService
{
    public Task DeleteOldTimeSlotAsync();
}
