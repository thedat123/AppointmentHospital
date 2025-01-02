using System;

namespace AppointmentHospital.Models;

public class DaySchedule
{
    public bool Enabled { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}
