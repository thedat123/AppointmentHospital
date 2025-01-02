using System;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Models;

public class RegularScheduleInput
{
    public List<DaySchedule> Schedules { get; set; }
}
