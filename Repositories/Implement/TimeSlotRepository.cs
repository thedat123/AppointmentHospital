using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories.Implement;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly AppDbContext _context;
    public TimeSlotRepository(AppDbContext context){
        _context = context; 
    }

    public async Task<Pagination<TimeSlot>> GetTimeSlotByDoctorId(Guid doctorId, int page){
        var timeSlots = _context.TimeSlots.Where(x => x.DoctorId == doctorId);
        var paginatedTimeSlots = await Pagination<TimeSlot>.PaginatedList(timeSlots, page);
        return paginatedTimeSlots;
    }

    public void AddTimeSlot(TimeSlot timeSlot){

        _context.TimeSlots.Add(timeSlot);
        _context.SaveChanges();
    }

    public List<DateTime> GetRemainingDaysInMonth(DateTime startDate, DateTime endOfMonth, List<DaySchedule> schedules)
    {
        var enabledDays = schedules.
            Where(x => x.Enabled)
            .Select(s => s.DayOfWeek)
            .ToList();

        return Enumerable
            .Range(0, (endOfMonth - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .Where(date => enabledDays.Contains(date.DayOfWeek)) 
            .ToList();
    }

    public void UpdateTimeSlotAvalableStatusByTimeSlotID(Guid timeSlotID){
        var timeSlot = _context.TimeSlots.Find(timeSlotID);
        if(timeSlot != null){
            timeSlot.Available = false;
        }
        _context.SaveChanges();
    }

    public List<TimeSlot> GetTimeSlotsByDoctorAndDate(Guid id, DateTime date){
        return _context.TimeSlots
        .Where(x => x.DoctorId == id && x.StartTime.Date == date.Date)
        .ToList();
    }

    public void DeleteTimeSlot(Guid timeSlotId)
    {
        var timeSlot = _context.TimeSlots.Find(timeSlotId);
        if (timeSlot != null)
        {
            _context.TimeSlots.Remove(timeSlot);
            _context.SaveChanges();
        }
        else
        {
            throw new ArgumentException("TimeSlot not found with the specified ID.");
        }
    }

    public TimeSlot GetTimeSlotById(Guid timeSlotId){
        return _context.TimeSlots.Find(timeSlotId);
    }

    public List<TimeSlot> GetAllTimeSlotByParticularDate(DateTime date){
        return _context.TimeSlots.Where(x => x.StartTime.Date == date || x.EndTime.Date == date).ToList();
    }
}
