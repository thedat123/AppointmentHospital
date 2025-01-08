using System;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class CronTimeSlotService : ICronTimeSlotService
{
    private readonly ITimeSlotRepository _timeSlotRepository;
    public CronTimeSlotService(ITimeSlotRepository timeSlotRepository){
        _timeSlotRepository = timeSlotRepository;
    }

    public Task DeleteOldSchedules()
    {
        _timeSlotRepository.DeleteOldTimeSlot();
        Console.WriteLine($"Deleted schedule(s) from yesterday at {DateTime.Now}");
        return Task.CompletedTask;
    }
}
