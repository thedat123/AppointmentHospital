using AppointmentHospital.DTOs.Patient;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using AppointmentHospital.Helpers;
using AppointmentHospital.DTOs.TimeSlot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using System.Security.Claims;
using AppointmentHospital.EnumStatus;
using Hangfire;
using AppointmentHospital.Services.Implement;

namespace AppointmentHospital.Controllers
{
    public class PatientController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _appDbContext;
        private readonly IAppointmentDateService _appointmentDateService;
        private readonly ILogger<PatientController> _logger;
        private readonly IEmailService _emailService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IHubContext<ScheduleHub> _hubContext;
        private readonly IDiseasePredictionService _predictionService;

        public PatientController(AppDbContext appDbContext,IPatientService patientService ,IDoctorService doctorService, ILogger<PatientController> logger, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, IEmailService emailService, ITimeSlotService timeSlotService, IHubContext<ScheduleHub> hubContext, IDiseasePredictionService diseasePredictionService)
        {
            _appDbContext = appDbContext;
            _patientService = patientService;
            _doctorService = doctorService;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _appointmentDateService = appointmentDateService;
            _emailService = emailService;
            _timeSlotService = timeSlotService;
            _hubContext = hubContext;
            _predictionService = diseasePredictionService;
        }

        public IActionResult Index()
        {
            List<Doctor> doctors = _doctorService.getAllDoctors();
            return View(doctors);
        }

        [HttpPost]
        public IActionResult BookSchedule(Guid DoctorId, string selectedDate, string selectedTime, string TimeSlotId)
        {
            DateTime appointmentDateTime = DateTime.Parse($"{selectedDate} {selectedTime}");
            string doctorName = _doctorService.getDoctorNameByDoctorId(DoctorId);

            var model = new
            {
                DoctorName = doctorName,
                AppointmentDateTime = appointmentDateTime,
                DoctorId = DoctorId,
                TimeSlotId = Guid.Parse(TimeSlotId)
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> PersonalProfile()
        {
            if(User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientById(Guid.Parse(userId));
                ViewBag.PatientInfo = patient;
                return View(new PatientRequest());
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PersonalProfile(Guid patientId, PatientRequest request)
        {
            var patient = await _patientService.EditPatientInfo(patientId ,request);
            await _hubContext.Clients.All.SendAsync("UpdatePatientProfile", patient);
            ViewBag.PatientInfo = patient;
            return View(new PatientRequest());
        }

        [HttpGet]
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
        public async Task<IActionResult> BookForSelf(Guid DoctorId, DateTime AppointmentDateTime, string symptoms, Guid TimeSlotId)
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            if (string.IsNullOrEmpty(patientId))
            {
                TempData["ErrorMessage"] = "Patient ID is not available. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            var doctor = _doctorService.getDoctorById(DoctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Index", "Patient");
            }

            var patient = await _patientService.GetPatientById(Guid.Parse(patientId));
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Index", "Patient");
            }

            var appointment = new Appointment
            {
                DoctorId = DoctorId,
                PatientId = Guid.Parse(patientId),
                AppointmentTime = AppointmentDateTime,
                Symptoms = symptoms,
            };

            try
            {
                _appointmentDateService.AddAppointment(appointment);
                _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(TimeSlotId);
                string body = await _emailService.GetBookingTemplate(appointment.AppointmentTime, doctor.FullName, patient.FullName);
                await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body);
                TempData["SuccessMessage"] = "Your appointment has been booked successfully!";

                var updatedDate = appointment.AppointmentTime.Date.ToString("yyyy-MM-dd");
                await _hubContext.Clients.All.SendAsync("ScheduleUpdated", DoctorId, updatedDate);
                await _hubContext.Clients.All.SendAsync("UpdateStatistics");
                return RedirectToAction("Index", "Patient");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while booking the appointment: {ex.Message}";
                return RedirectToAction("Index", "Patient");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BookForOther(Guid DoctorId, DateTime AppointmentDateTime, string acquaintanceName, string gender, string symptom, Guid TimeSlotId, DateTime birthDate, int identificationNumber, string address)
        {
            var doctor = _doctorService.getDoctorById(DoctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Index", "Patient");
            }

            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            var patient = await _patientService.GetPatientById(Guid.Parse(patientId));
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Index", "Patient");
            }

            var aquaintance = new Acquaintance
            {
                Name = acquaintanceName,
                DateOfBirth = birthDate,
                Gender = gender,
                IdentificationNumber = identificationNumber,
                Address = address,
                PatientId = Guid.Parse(patientId)
            };

            _patientService.AddAcquaintance(aquaintance);

            if (string.IsNullOrEmpty(symptom))
            {
                TempData["ErrorMessage"] = "Symptoms cannot be empty.";
                Console.WriteLine("Symptoms cannot be empty.");
                return RedirectToAction("Index", "Patient");
            }

            var appointment = new Appointment
            {
                DoctorId = DoctorId,
                PatientId = Guid.Parse(patientId),
                AppointmentTime = AppointmentDateTime,
                Symptoms = symptom,
                AcquaintanceId = aquaintance.Id
            };

            Console.WriteLine($"Identification Number: {identificationNumber}, Address: {address}");

            try
            {
                _appointmentDateService.AddAppointment(appointment);
                _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(TimeSlotId);
                string body = await _emailService.GetBookingTemplate(appointment.AppointmentTime, doctor.FullName, patient.FullName);
                await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body);

                TempData["SuccessMessage"] = "Appointment for acquaintance has been booked successfully!";

                var updatedDate = appointment.AppointmentTime.Date.ToString("yyyy-MM-dd");
                await _hubContext.Clients.All.SendAsync("ScheduleUpdated", DoctorId, updatedDate);
                return RedirectToAction("Index", "Patient");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while booking the appointment: {ex.Message}";
                return RedirectToAction("Index", "Patient");
            }
        }

        [HttpGet("/api/schedule")]
        public IActionResult GetSchedule(Guid id, DateTime date, bool isApiRequest = false)
        {
            if (isApiRequest)
            {
                var timeSlots = _timeSlotService.GetTimeSlotsByDoctorAndDate(id, date)
                    .Select(ts => new TimeSlotDto
                    {
                        Id = ts.TimeSlotId,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        Available = ts.Available
                    }).ToList();
                return Json(new { values = timeSlots });
            }
            return View();
        }

        public IActionResult MySchedule(AppointmentStatus? status = null)
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            if (patientId != null)
            {
                var allAppointments = _appointmentDateService.GetAppointmentsByPatientId(Guid.Parse(patientId));

                var filteredAppointments = status.HasValue
                ? _appointmentDateService.GetAppointmentsByPatientId(Guid.Parse(patientId), status.Value)
                : allAppointments;

                return View(filteredAppointments); 
            }
            return RedirectToAction("Index", "Patient");
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var appointment = _appointmentDateService.GetAppointmentsById(id);
            if (appointment == null || appointment.Status != AppointmentStatus.Pending)
            {
                return NotFound("Appointment not found or not in pending status.");
            }

            if ((appointment.AppointmentTime - DateTime.Now).TotalHours > 24)
            {
                return BadRequest("Cannot cancel appointments more than 24 hours in advance.");
            }

            _appointmentDateService.UpdateStatusAppointment(id, AppointmentStatus.Canceled);
            await _hubContext.Clients.All.SendAsync("UpdateStatus", appointment.AppointmentId, AppointmentStatus.Canceled);

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction("MySchedule");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromForm] FeedbackRequest request )
        {
            await _patientService.AddFeedback(request);
            await _hubContext.Clients.All.SendAsync("UpdateFeedback");
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetFeedback(Guid id) {
            var feedback = await _patientService.GetFeedback(id);
            return Json(feedback);
        }
        [HttpGet]
        public async Task<IActionResult> HasFeedback(Guid id)
        {
            var result = await _patientService.HasFeedback(id);
            return Json(result);
        }
        public async Task<ActionResult> Predict(string[] symptoms)
        {
            if (symptoms == null || symptoms.Length == 0)
            {
                TempData["Error"] = "No symptoms provided.";
                return RedirectToAction("Index", "Patient");
            }

            var result = await _predictionService.PredictDiseaseAsync(symptoms);

            if (result != null)
            {
                TempData["Disease"] = result.disease.ToString();
                TempData["ProbabilityPercentage"] = (Convert.ToDouble(result.probability) * 100).ToString("F2");
                TempData["Probability"] = TempData["ProbabilityPercentage"];
                TempData["Description"] = result.description.ToString();
                TempData["Precautions"] = result.precautions?.ToString();
            }
            else
            {
                TempData["Error"] = "Failed to fetch prediction.";
            }

            return RedirectToAction("Index", "Patient");
        }


    }

}
