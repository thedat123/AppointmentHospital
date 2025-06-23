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
    private readonly ITimeSlotService _timeSlotService;
    public CollaboratorController(IAppointmentDateService appointmentDateService, IPatientService patientService, IEmailService emailService, IHubContext<ScheduleHub> hubContext, ITimeSlotService timeSlotService)
    {
        this._appointmentDateService = appointmentDateService;
        this._patientService = patientService;
        this._emailService = emailService;
        this._hubContext = hubContext;
        this._timeSlotService = timeSlotService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1, 
        int? status = null, 
        string searchTerm = null, 
        DateTime? appointmentDate = null)
    {
        // Get filtered and paginated appointments
        var appointments = await _appointmentDateService.GetAllPendingAppointments(
                page, 
                status, 
                searchTerm, 
                appointmentDate);

        ViewBag.CurrentSearchTerm = searchTerm;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentDate = appointmentDate?.ToString("yyyy-MM-dd");
        ViewBag.CurrentPage = page;
        return View(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, int status)
    {
        var appointment = _appointmentDateService.GetAppointmentsById(id);
        var patient = await _patientService.GetPatientById(appointment.PatientId);
        if ((AppointmentStatus)status == AppointmentStatus.Canceled)
        {
            var timeSlotId = _timeSlotService.GetTimeSlotIdByAppointmentime(appointment.AppointmentTime);
            _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(timeSlotId, true);
            string body = await _emailService.GetCancelledTemplate(
                appointment.AppointmentTime,
                appointment.Doctor.FullName,
                appointment.Acquaintance?.Name ?? appointment.Patient.FullName
            );
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body ));
        }
        else if ((AppointmentStatus)status == AppointmentStatus.Confirmed)
        {
            string body = await _emailService.GetConfirmedTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Acquaintance?.Name ?? appointment.Patient.FullName);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body ));
        }
        _appointmentDateService.UpdateStatusAppointment(id, (AppointmentStatus)status);
        await _hubContext.Clients.All.SendAsync("UpdateStatus", appointment.AppointmentId, (AppointmentStatus)status);
        return RedirectToAction("Index");
    }
}
