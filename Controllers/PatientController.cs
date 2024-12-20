using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Controllers
{
    public class PatientController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IAppointmentDateService _appointmentDateService;
        private readonly ILogger<PatientController> _logger;

        // Constructor duy nhất
        public PatientController(IDoctorService doctorService, ILogger<PatientController> logger, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService)
        {
            _doctorService = doctorService;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _appointmentDateService = appointmentDateService;
        }

        public IActionResult Index()
        {
            List<Doctor> doctors = _doctorService.getAllDoctors();
            return View(doctors);
        }

        [HttpPost]
        public IActionResult BookSchedule(Guid DoctorId, string selectedDate, string selectedTime)
        {
            DateTime appointmentDateTime = DateTime.Parse($"{selectedDate} {selectedTime}");
            string doctorName = _doctorService.getDoctorNameByDoctorId(DoctorId);

            var model = new
            {
                DoctorName = doctorName,
                AppointmentDateTime = appointmentDateTime,
                DoctorId = DoctorId
            };

            return View(model);
        }

        public IActionResult DetailDoctor(Guid id)
        {
            Doctor doctor = _doctorService.getDoctorById(id);
            List<TimeSlot> timeSlots = _doctorService.getTimeSlotByDoctorId(id);

            var viewModel = new DoctorDetailViewModel
            {
                Doctor = doctor,
                TimeSlots = timeSlots
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult BookForSelf(Guid DoctorId, DateTime AppointmentDateTime, String symptoms)
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            var appointment = new Appointment
            {
                DoctorId = DoctorId,
                PatientId = patientId != null ? Guid.Parse(patientId) : Guid.Empty,
                AppointmentTime = AppointmentDateTime,
                Symptoms = symptoms
            };
            _appointmentDateService.AddAppointment(appointment);
            return RedirectToAction("Index", "Patient");
        }

        public IActionResult MySchedule()
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            if (patientId != null)
            {
                var appointments = _appointmentDateService.GetAppointmentsByPatientId(Guid.Parse(patientId));
                return View(appointments); 
            }
            return RedirectToAction("Index", "Patient");
        }
    }

}
