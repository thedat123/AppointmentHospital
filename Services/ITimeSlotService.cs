using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface ITimeSlotService
{
    public Task<Pagination<TimeSlot>> GetTimeSlotByDoctorId(Guid doctorId, int page, string sortBy, string sortOrder, DateTime? filterDate);
    public void AddTimeSlot(TimeSlot timeSlot);
    public List<DateTime> GetRemainingDaysInMonth(DateTime startDate, DateTime endOfMonth, List<DaySchedule> schedules);
    public void UpdateTimeSlotAvalableStatusByTimeSlotID(Guid timeSlotID, bool status);
    public List<TimeSlot> GetTimeSlotsByDoctorAndDate(Guid id, DateTime date);
    public void DeleteTimeSlot(Guid timeSlotId);
    public TimeSlot GetTimeSlotById(Guid timeSlotId);
    public List<TimeSlot> GetAllTimeSlotByParticularDate(DateTime date);
    public void UpdateNoteInTimeSlot(Guid timeSlotId, string note);
    public Guid GetTimeSlotIdByAppointmentime(DateTime appointmentTime);
}
