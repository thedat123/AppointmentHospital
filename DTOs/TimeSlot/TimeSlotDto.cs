using System;

namespace AppointmentHospital.DTOs.TimeSlot;

public class TimeSlotDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Available { get; set; }
}
