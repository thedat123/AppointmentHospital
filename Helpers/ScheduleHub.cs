using System;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using Microsoft.AspNetCore.SignalR;

namespace AppointmentHospital.Helpers;

public class ScheduleHub : Hub
{
    public async Task NotifyScheduleUpdated(int doctorId, DateTime selectedDate)
    {
        await Clients.All.SendAsync("ScheduleUpdated", doctorId, selectedDate.ToString("yyyy-MM-dd"));
    }

    public async Task NotifyAppointmentStatus(int appointmentId, AppointmentStatus status)
    {
        await Clients.All.SendAsync("UpdateStatus", appointmentId, status);
    }

    public async Task NotifyPatientInfoUpdated(Patient patient){
        await Clients.All.SendAsync("UpdatePatientProfile", patient);
    }

}
