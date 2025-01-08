using System.Security.Claims;
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using AppointmentHospital.ViewModels;
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
        private readonly IManagingDoctorService _managingDoctorService;
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, ITimeSlotService timeSlotService, IEmailService emailService, IPatientService patientService, IManagingDoctorService managingDoctorService ,IHubContext<ScheduleHub> hubContext)
        {
            _logger = logger;
            this.doctorService = doctorService;
            this._contextAccessor = contextAccessor;
            this._appointmentDateService = appointmentDateService;
            this._timeSlotService = timeSlotService;
            this._emailService = emailService;
            this._patientService = patientService;
            this._hubContext = hubContext;
            this._managingDoctorService = managingDoctorService;
        }

        public async Task<IActionResult> Index(AppointmentStatus? status = AppointmentStatus.Pending, int page = 1)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");

            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var allAppointments = await _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId), page);

            var stats = new
            {
                PendingCount =  allAppointments.Count(a => a.Status == AppointmentStatus.Pending),
                ConfirmedCount = allAppointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                CompletedCount = allAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                CanceledCount = allAppointments.Count(a => a.Status == AppointmentStatus.Canceled)
            };

            var filteredAppointments = status.HasValue
                ? await _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId), status.Value, page)
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

        public async Task<IActionResult> Calendar(int page = 1)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            ViewBag.DoctorName = doctor.FullName ?? "Unknown Doctor";
            ViewBag.Speciality = doctor.Specializaiton.GetDisplayName().ToString() ?? "Unknown Speciality";

            var timeSlot = await _timeSlotService.GetTimeSlotByDoctorId(Guid.Parse(doctorId), page);
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

        public IActionResult StartDiagnosis(Guid id)
        {
            var appointment = _appointmentDateService.GetAppointmentsById(id);
            var diagnosisHistory = new List<DiagnosisHistory>();

            if (appointment.AcquaintanceId == null || appointment.AcquaintanceId == Guid.Empty)
            {
                diagnosisHistory = _patientService.GetDiagnosisHistoriesByPatientId(appointment.PatientId);
            }
            else
            {
                diagnosisHistory = _patientService.GetDiagnosisHistoriesByAcquaintanceId(appointment.AcquaintanceId.Value);
            }

            ViewData["DiagnosisHistory"] = diagnosisHistory;
            return View(appointment);
        }

        public IActionResult PersonalInfo()
         {
            if(User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)){
                var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
                ViewBag.Specializaiton = _managingDoctorService.GetSpecialization();
                ViewBag.DoctorId = doctorId;
                return View(doctor);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Doctor request, string phoneNumber, Specialization specialization) {
             var doctor = await  doctorService.updateDoctor(request, phoneNumber);
             ViewBag.Specializaiton = _managingDoctorService.GetSpecialization();
             ViewBag.DoctorId = doctor.DoctorId;

             await _hubContext.Clients.All.SendAsync("UpdateDoctorProfile", doctor);
             return View("PersonalInfo", doctor);
        } 

        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query cannot be empty.");
            }

            var searchDrugName = doctorService.GetDrugNameSearch(query);
            if (searchDrugName == null || !searchDrugName.Any())
            {
                return Ok(new List<string>());
            }
            return Ok(searchDrugName);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDiagnosis(Guid AppointmentId, Guid PatientId, Guid DoctorId, Guid AcquaintanceId, DateTime DateTime, string DiagnosisDetails, string PrescribedMedications, string DoctorNotes){
            List<string> prescribedMedicationList = PrescribedMedications?.Split(',').ToList() ?? new List<string>();
            DiagnosisHistory diagnosisHistory = new DiagnosisHistory{
                AppointmentId = AppointmentId,
                PatientId = PatientId,
                DoctorId = DoctorId,
                AcquaintanceId = AcquaintanceId,
                DateTime = DateTime,
                Diagnosis = DiagnosisDetails,
                Prescription = prescribedMedicationList,
                DoctorNote = DoctorNotes,
            };

            doctorService.AddDiagnosticHistory(diagnosisHistory);

            List<(string Medication, int Quantity)> processedMedications = PrescribedMedications?
            .Split(',')
            .Select(item =>
            {
                var parts = item.Split('-');
                return (
                    Medication: parts[0],
                    Quantity: parts.Length > 1 && int.TryParse(parts[1], out var qty) ? qty : 1
                );
            }).ToList() ?? new List<(string, int)>();

            string PrescribedMedicationsHtml = string.Join("", processedMedications.Select(m =>
                $"<li><strong>Tên thuốc:</strong> {m.Medication}<br><strong>Số lượng:</strong> {m.Quantity}</li>"
            ));

            var patient = await _patientService.GetPatientById(PatientId);

            string body = await _emailService.GetCompletedTemplate(patient.FullName, doctorService.getDoctorById(DoctorId).FullName, DiagnosisDetails, PrescribedMedicationsHtml, DoctorNotes);
            await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body);

            _appointmentDateService.UpdateStatusAppointment(AppointmentId, AppointmentStatus.Completed);
            await _hubContext.Clients.All.SendAsync("UpdateStatus", AppointmentId, AppointmentStatus.Completed);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteTimeSlot(Guid id){
            var timeSlot = _timeSlotService.GetTimeSlotById(id);
            _timeSlotService.DeleteTimeSlot(id);
            return RedirectToAction("Calendar");
        }

        [HttpPost]
        public IActionResult RegisterOffDay(DateTime offDate, string Note)
        {
            var timeList = _timeSlotService.GetAllTimeSlotByParticularDate(offDate);

            if (timeList.Count != 0)
            {
                bool hasAvailable = true;
                List<Guid> notifyPatientSlots = new List<Guid>();

                foreach (var timeSlot in timeList)
                {
                    if (timeSlot.Available)
                    {
                        _timeSlotService.UpdateNoteInTimeSlot(timeSlot.TimeSlotId, Note);
                        _timeSlotService.DeleteTimeSlot(timeSlot.TimeSlotId);
                    }
                    else
                    {
                        hasAvailable = false;
                        notifyPatientSlots.Add(timeSlot.TimeSlotId);
                    }
                }

                if (!hasAvailable)
                {
                    TempData["ShowSuggestModal"] = true;
                    TempData["timeSlotIds"] = JsonConvert.SerializeObject(notifyPatientSlots);
                    TempData["offDate"] = offDate;
                    TempData["note"] = Note;
                    return RedirectToAction("Calendar");
                }

                TempData["SuccessMessage"] = "Off day registered successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No time slots available for the selected date.";
            }

            return RedirectToAction("Calendar");
        }

        [HttpGet]
        public async Task<IActionResult> SuggestDayForMultiplePatients(DateTime suggestDate)
        {
            var timeSlotIdsJson = TempData["timeSlotIds"]?.ToString();
            var offDateStr = TempData["offDate"]?.ToString();
            var note = TempData["note"]?.ToString();

            if (string.IsNullOrEmpty(timeSlotIdsJson) || string.IsNullOrEmpty(offDateStr))
            {
                return RedirectToAction("Calendar");
            }

            var timeSlotIds = JsonConvert.DeserializeObject<List<Guid>>(timeSlotIdsJson);
            var offDate = DateTime.Parse(offDateStr);

            var doctorId = Guid.Parse(_contextAccessor.HttpContext?.Session.GetString("DoctorId")!);
            var patientEmails = new HashSet<string>();

            foreach (var timeSlotId in timeSlotIds)
            {
                var timeSlot = _timeSlotService.GetTimeSlotById(timeSlotId);

                if (timeSlot != null)
                {
                    var appointments = _appointmentDateService.GetAppointmentsByDoctorIdAndDate(doctorId, offDate);

                    foreach (var appointment in appointments)
                    {
                        var patient = await _patientService.GetPatientById(appointment.PatientId);

                        if (patient != null && !patientEmails.Contains(patient.EmailAddress))
                        {
                            patientEmails.Add(patient.EmailAddress);

                            _appointmentDateService.UpdateStatusAppointment(appointment.AppointmentId, AppointmentStatus.Canceled);
                            string body = await _emailService.GetCancelAndSuggestTemplate(
                                appointment.AppointmentTime,
                                appointment.Doctor.FullName,
                                patient.FullName,
                                suggestDate
                            );

                            await _emailService.SendMailAsync(
                                patient.EmailAddress,
                                $"Medical Appointment Of ({patient.FullName})",
                                body
                            );
                        }
                    }

                    _timeSlotService.DeleteTimeSlot(timeSlot.TimeSlotId);
                }
            }

            return RedirectToAction("Calendar");
        }


        [HttpPost]
        public async Task<IActionResult> SuggestDay(Guid timeSlotId, DateTime suggestDate){
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var timeSlot = _timeSlotService.GetTimeSlotById(timeSlotId);

            var appointment = _appointmentDateService.GetAppointmentsByDoctorIdAndStartTime(Guid.Parse(doctorId), timeSlot.StartTime);
            var patient = await _patientService.GetPatientById(appointment.PatientId);

            if(appointment != null){
                _appointmentDateService.UpdateStatusAppointment(appointment.AppointmentId, AppointmentStatus.Canceled);
                _timeSlotService.DeleteTimeSlot(timeSlot.TimeSlotId);

                string body = await _emailService.GetCancelAndSuggestTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName, suggestDate);
                await _emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body);
            }

            return RedirectToAction("Calendar");
        }
    }
}
