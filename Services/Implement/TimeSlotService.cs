using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;
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

    public void AddTimeSlot(TimeSlot timeSlot)
    {
        timeSlotRepository.AddTimeSlot(timeSlot);
    }

    public List<DateTime> GetRemainingDaysInMonth(DateTime startDate, DateTime endOfMonth, List<DaySchedule> schedules){
        return timeSlotRepository.GetRemainingDaysInMonth(startDate, endOfMonth, schedules);
    }

    public void UpdateTimeSlotAvalableStatusByTimeSlotID(Guid timeSlotID){
        timeSlotRepository.UpdateTimeSlotAvalableStatusByTimeSlotID(timeSlotID);
    }
    public List<TimeSlot> GetTimeSlotsByDoctorAndDate(Guid id, DateTime date){
        return timeSlotRepository.GetTimeSlotsByDoctorAndDate(id, date);
    }
}
