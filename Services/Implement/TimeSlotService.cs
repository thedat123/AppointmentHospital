using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class TimeSlotService : ITimeSlotService
{
    private readonly ITimeSlotRepository timeSlotRepository;

    public TimeSlotService(ITimeSlotRepository _timeslotRepository){
        timeSlotRepository = _timeslotRepository;
    }

    public async Task<Pagination<TimeSlot>> GetTimeSlotByDoctorId(Guid doctorId, int page, string sortBy, string sortOrder, DateTime? filterDate)
    {
        return await timeSlotRepository.GetTimeSlotByDoctorId(doctorId, page, sortBy, sortOrder, filterDate);
    }

    public void AddTimeSlot(TimeSlot timeSlot)
    {
        timeSlotRepository.AddTimeSlot(timeSlot);
    }

    public List<DateTime> GetRemainingDaysInMonth(DateTime startDate, DateTime endOfMonth, List<DaySchedule> schedules){
        return timeSlotRepository.GetRemainingDaysInMonth(startDate, endOfMonth, schedules);
    }

    public void UpdateTimeSlotAvalableStatusByTimeSlotID(Guid timeSlotID, bool status){
        timeSlotRepository.UpdateTimeSlotAvalableStatusByTimeSlotID(timeSlotID, status);
    }
    public List<TimeSlot> GetTimeSlotsByDoctorAndDate(Guid id, DateTime date){
        return timeSlotRepository.GetTimeSlotsByDoctorAndDate(id, date);
    }

    public void DeleteTimeSlot(Guid timeSlotId){
        timeSlotRepository.DeleteTimeSlot(timeSlotId);
    }

    public TimeSlot GetTimeSlotById(Guid timeSlotId){
        return timeSlotRepository.GetTimeSlotById(timeSlotId);
    }

    public List<TimeSlot> GetAllTimeSlotByParticularDate(DateTime date){
        return timeSlotRepository.GetAllTimeSlotByParticularDate(date);
    }
    public void UpdateNoteInTimeSlot(Guid timeSlotId, string note){
        timeSlotRepository.UpdateNoteInTimeSlot(timeSlotId, note);
    }

    public Guid GetTimeSlotIdByAppointmentime(DateTime appointmentTime){
        return timeSlotRepository.GetTimeSlotIdByAppointmentime(appointmentTime);
    }
}
