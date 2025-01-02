using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace AppointmentHospital.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService doctorService;
        private readonly ILogger<DoctorController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAppointmentDateService _appointmentDateService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IEmailService _emailService;
        private readonly IPatientService _patientService;
        private readonly IHubContext<ScheduleHub> _hubContext;
        
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, ITimeSlotService timeSlotService, IEmailService emailService, IPatientService patientService, IHubContext<ScheduleHub> hubContext)
        {
            _logger = logger;
            this.doctorService = doctorService;
            this._contextAccessor = contextAccessor;
            this._appointmentDateService = appointmentDateService;
            this._timeSlotService = timeSlotService;
            this._emailService = emailService;
            this._patientService = patientService;
            this._hubContext = hubContext;
        }

        public IActionResult Index(AppointmentStatus? status = AppointmentStatus.Pending)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");

            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var allAppointments = _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId));

            var stats = new
            {
                PendingCount = allAppointments.Count(a => a.Status == AppointmentStatus.Pending),
                ConfirmedCount = allAppointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                CompletedCount = allAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                CanceledCount = allAppointments.Count(a => a.Status == AppointmentStatus.Canceled)
            };

            var filteredAppointments = status.HasValue
                ? _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId), status.Value)
                : allAppointments;

            ViewBag.Stats = stats;
            ViewBag.DoctorName = doctor.FullName ?? "Unknown Doctor";
            ViewBag.SelectedStatus = status; 

            return View(filteredAppointments);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, int status)
        {
            var appointment = _appointmentDateService.GetAppointmentsById(id);
            var patient = await _patientService.GetPatientById(appointment.PatientId);
            if((AppointmentStatus)status == AppointmentStatus.Canceled){
                string body = await _emailService.GetCancelledTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName);
                await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body);
            } else if((AppointmentStatus)status == AppointmentStatus.Confirmed){
                string body = await _emailService.GetConfirmedTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName);
                await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body);
            }

            _appointmentDateService.UpdateStatusAppointment(id, (AppointmentStatus)status);
            await _hubContext.Clients.All.SendAsync("UpdateStatus", appointment.AppointmentId, (AppointmentStatus)status);
            return RedirectToAction("Index");
        }

        public IActionResult Calendar()
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            ViewBag.DoctorName = doctor.FullName ?? "Unknown Doctor";
            ViewBag.Speciality = doctor.Specializaiton.GetDisplayName().ToString() ?? "Unknown Speciality";

            var timeSlot = _timeSlotService.GetTimeSlotByDoctorId(Guid.Parse(doctorId));
            return View(timeSlot);
        }

        [HttpPost]
        public IActionResult UpdateRegularSchedule(RegularScheduleInput input)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");

            if (string.IsNullOrEmpty(doctorId))
            {
                TempData["ErrorMessage"] = "Doctor ID is required.";
                return RedirectToAction("Calendar");
            }

            if (input.Schedules == null || !input.Schedules.Any())
            {
                TempData["ErrorMessage"] = "No schedule data received.";
                return RedirectToAction("Calendar");
            }

            DateTime now = DateTime.Now;
            DateTime startDate = now.Date;
            DateTime endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var remainingDays = _timeSlotService.GetRemainingDaysInMonth(startDate, endOfMonth, input.Schedules);

            foreach (var day in remainingDays)
            {
                var schedule = input.Schedules.FirstOrDefault(s =>
                    s.DayOfWeek == day.DayOfWeek &&
                    s.StartTime.HasValue &&
                    s.EndTime.HasValue);

                if (schedule != null)
                {
                    TimeSpan currentStartTime = schedule.StartTime.Value;
                    while (currentStartTime < schedule.EndTime.Value)
                    {
                        TimeSpan nextHour = currentStartTime.Add(TimeSpan.FromHours(1));

                        _timeSlotService.AddTimeSlot(new TimeSlot
                        {
                            TimeSlotId = Guid.NewGuid(),
                            DoctorId = Guid.Parse(doctorId),
                            StartTime = day.Date.Add(currentStartTime),
                            EndTime = day.Date.Add(nextHour),
                            Available = true
                        });
                        
                        currentStartTime = nextHour;
                    }
                }
            }

            TempData["SuccessMessage"] = "Update successful!";
            return RedirectToAction("Calendar");
        }

        public IActionResult StartDiagnosis(Guid id){
            var appointment = _appointmentDateService.GetAppointmentsById(id);
            return View(appointment);
        }
    }
}
