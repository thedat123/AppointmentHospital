using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;

namespace AppointmentHospital.Repositories.Implement;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly AppDbContext _context;
    public TimeSlotRepository(AppDbContext context){
        _context = context; 
    }

    public List<TimeSlot> GetTimeSlotByDoctorId(Guid doctorId){
        return _context.TimeSlots.Where(x => x.DoctorId == doctorId).ToList();
    }
}
