using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class TimeSlotService : ITimeSlotService
{
    private readonly ITimeSlotRepository timeSlotRepository;

    public TimeSlotService(ITimeSlotRepository _timeslotRepository){
        timeSlotRepository = _timeslotRepository;
    }

    public List<TimeSlot> GetTimeSlotByDoctorId(Guid doctorId)
    {
        return timeSlotRepository.GetTimeSlotByDoctorId(doctorId);
    }
}
