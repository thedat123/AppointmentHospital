using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, ITimeSlotService timeSlotService)
        {
            _logger = logger;
            this.doctorService = doctorService;
            this._contextAccessor = contextAccessor;
            this._appointmentDateService = appointmentDateService;
            this._timeSlotService = timeSlotService;
        }

        public IActionResult Index(AppointmentStatus? status)
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
                ? allAppointments.Where(a => a.Status == status.Value).ToList()
                : allAppointments;

            ViewBag.Stats = stats;
            ViewBag.DoctorName = doctor.FullName ?? "Unknown Doctor";
            ViewBag.SelectedStatus = status; 

            return View(filteredAppointments);
        }


        [HttpPost]
        public IActionResult UpdateStatus(Guid id, int status)
        {
            _appointmentDateService.UpdateStatusAppointment(id, (AppointmentStatus)status);
            return RedirectToAction("Index");
        }

        public IActionResult Calendar()
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var timeSlot = _timeSlotService.GetTimeSlotByDoctorId(Guid.Parse(doctorId));
            return View(timeSlot);
        }
    }
}
