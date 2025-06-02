using System;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Services;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AppointmentHospital.Controllers;

[Authorize(Roles = "Collaborator")] 
public class CollaboratorController : Controller
{
    private readonly IAppointmentDateService _appointmentDateService;
    private readonly IPatientService _patientService;
    private readonly IEmailService _emailService;
    private readonly IHubContext<ScheduleHub> _hubContext;
    public CollaboratorController(IAppointmentDateService appointmentDateService, IPatientService patientService, IEmailService emailService, IHubContext<ScheduleHub> hubContext)
    {
        this._appointmentDateService = appointmentDateService;
        this._patientService = patientService;
        this._emailService = emailService;
        this._hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var appointments = await _appointmentDateService.GetAllPendingAppointments(page);
        return View(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, int status)
    {
        var appointment = _appointmentDateService.GetAppointmentsById(id);
        var patient = await _patientService.GetPatientById(appointment.PatientId);
        if ((AppointmentStatus)status == AppointmentStatus.Canceled)
        {
            string body = await _emailService.GetCancelledTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body ));
        }
        else if ((AppointmentStatus)status == AppointmentStatus.Confirmed)
        {
            string body = await _emailService.GetConfirmedTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body ));
        }
        _appointmentDateService.UpdateStatusAppointment(id, (AppointmentStatus)status);
        await _hubContext.Clients.All.SendAsync("UpdateStatus", appointment.AppointmentId, (AppointmentStatus)status);
        return RedirectToAction("Index");
    }

}
