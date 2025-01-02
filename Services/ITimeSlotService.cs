using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface ITimeSlotService
{
    public List<TimeSlot> GetTimeSlotByDoctorId(Guid doctorId);
    public void AddTimeSlot(TimeSlot timeSlot);
    public List<DateTime> GetRemainingDaysInMonth(DateTime startDate, DateTime endOfMonth, List<DaySchedule> schedules);
    public void UpdateTimeSlotAvalableStatusByTimeSlotID(Guid timeSlotID);
    public List<TimeSlot> GetTimeSlotsByDoctorAndDate(Guid id, DateTime date);
}
