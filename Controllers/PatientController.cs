using AppointmentHospital.DTOs.Patient;
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
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.Services;

namespace AppointmentHospital.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;
        private readonly IAppointmentDateService _appointmentDateService;
        private readonly ILogger<PatientController> _logger;
        private readonly IEmailService _emailService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IHubContext<ScheduleHub> _hubContext;
        private readonly IDiseasePredictionService _predictionService;
        private readonly ISpecialitiesService _specialitiesService;
        private readonly IManagingDoctorService _managingDoctorService;

        private readonly ChatbotService _chatbotService = new ChatbotService();

        public PatientController(AppDbContext appDbContext, IPatientService patientService, IDoctorService doctorService, ILogger<PatientController> logger, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, IEmailService emailService, ITimeSlotService timeSlotService, IHubContext<ScheduleHub> hubContext, IDiseasePredictionService diseasePredictionService, IManagingDoctorService managingDoctorService, ISpecialitiesService specialitiesService)
        {
            _context = appDbContext;
            _patientService = patientService;
            _doctorService = doctorService;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _appointmentDateService = appointmentDateService;
            _emailService = emailService;
            _timeSlotService = timeSlotService;
            _hubContext = hubContext;
            _predictionService = diseasePredictionService;
            _managingDoctorService = managingDoctorService;
            _specialitiesService = specialitiesService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? selectSpec, int page = 1, string searchTerm = null)
        {
            List<Doctor> doctors = await _doctorService.getAllDoctors(selectSpec, searchTerm, page);
            ViewBag.Specialization = _managingDoctorService.GetSpecialization();
            ViewBag.SelectedSpec = selectSpec;
            ViewBag.SearchTerm = searchTerm;
            return View(doctors);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ListDoctor(string? selectSpec, string? searchTerm, int page = 1)
        {
            List<Doctor> doctors = await _doctorService.getAllDoctors(selectSpec, searchTerm, page);
            ViewBag.SelectedSpec = selectSpec;
            ViewBag.SearchTerm = searchTerm;
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
            if (User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"User ID: {userId}");
                var patient = await _patientService.GetPatientById(Guid.Parse(userId));
                ViewBag.PatientInfo = patient;
                return View(new PatientRequest());
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PersonalProfile(Guid patientId, PatientRequest request)
        {
            var patient = await _patientService.EditPatientInfo(patientId, request);
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
                return Json(new { success = false, message = "Patient ID is not available. Please log in." });
            }

            var doctor = _doctorService.getDoctorById(DoctorId);
            if (doctor == null)
            {
                return Json(new { success = false, message = "Doctor not found." });
            }

            var patient = await _patientService.GetPatientById(Guid.Parse(patientId));
            if (patient == null)
            {
                return Json(new { success = false, message = "Patient not found." });
            }

            // Validate patient information
            if (!IsPatientInfoComplete(patient))
            {
                return Json(new
                {
                    success = false,
                    message = "Your profile information is incomplete. Please update your profile before booking an appointment."
                });
            }

            var timeSlot = _timeSlotService.GetTimeSlotById(TimeSlotId);
            if (timeSlot == null || !timeSlot.Available)
            {
                return Json(new
                {
                    success = false,
                    message = "This time slot is no longer available. Please choose another."
                });
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
                using (var transaction = _context.Database.BeginTransaction())
                {
                    _appointmentDateService.AddAppointment(appointment);
                    _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(TimeSlotId, false);

                    string body = await _emailService.GetBookingTemplate(appointment.AppointmentTime, doctor.FullName, patient.FullName);
                    string bodyRemind = await _emailService.GetRemindedTemplate(appointment.AppointmentTime, doctor.FullName, patient.FullName);
                    BackgroundJob.Enqueue<IEmailService>(emailservice => emailservice.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body));
                    var remindTime = appointment.AppointmentTime.Date.AddDays(-1).AddHours(20);
                    if (remindTime >= DateTime.Now)
                    {
                        BackgroundJob.Schedule<IEmailService>(emailservice => emailservice.SendMailAsync(patient.EmailAddress, $"Remind appointment Of ({patient.FullName})", bodyRemind), remindTime);
                    }

                    var updatedDate = appointment.AppointmentTime.Date.ToString("yyyy-MM-dd");
                    await _hubContext.Clients.All.SendAsync("ScheduleUpdated", DoctorId, updatedDate);
                    await _hubContext.Clients.All.SendAsync("TimeSlotBooked", DoctorId, updatedDate, TimeSlotId);
                    await _hubContext.Clients.All.SendAsync("UpdateStatistics");

                    transaction.Commit();
                    return Json(new { success = true, message = "Your appointment has been booked successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while booking the appointment: {ex.Message}" });
            }
        }

        private bool IsPatientInfoComplete(PatientResponse patient)
        {
            return !string.IsNullOrEmpty(patient.FullName) &&
                !string.IsNullOrEmpty(patient.PhoneNumber) &&
                patient.DateOfBirth != null &&
                !string.IsNullOrEmpty(patient.Address)
                && !string.IsNullOrEmpty(patient.IdentificationNumber);
        }

        [HttpPost]
        public async Task<IActionResult> BookForOther(Guid DoctorId, DateTime AppointmentDateTime, string acquaintanceName, string gender, string symptom, Guid TimeSlotId, DateTime birthDate, string identificationNumber, string address, string phoneNumber)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(acquaintanceName))
                    return Json(new { success = false, message = "Acquaintance name is required." });

                if (string.IsNullOrEmpty(symptom))
                    return Json(new { success = false, message = "Symptoms cannot be empty." });

                // Check doctor
                var doctor = _doctorService.getDoctorById(DoctorId);
                if (doctor == null)
                    return Json(new { success = false, message = "Doctor not found." });

                // Check patient
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
                if (string.IsNullOrEmpty(patientId))
                    return Json(new { success = false, message = "Patient session not found. Please log in again." });

                if (!Guid.TryParse(patientId, out var parsedPatientId))
                    return Json(new { success = false, message = "Invalid patient ID format." });

                var patient = await _patientService.GetPatientById(parsedPatientId);
                if (patient == null)
                    return Json(new { success = false, message = "Patient not found." });

                // Check time slot
                var timeSlot = _timeSlotService.GetTimeSlotById(TimeSlotId);
                if (timeSlot == null || !timeSlot.Available)
                    return Json(new { success = false, message = "This time slot is no longer available. Please choose another." });

                // Create acquaintance
                var acquaintance = new Acquaintance
                {
                    Name = acquaintanceName,
                    DateOfBirth = birthDate,
                    Gender = gender,
                    IdentificationNumber = identificationNumber,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    PatientId = parsedPatientId
                };

                _patientService.AddAcquaintance(acquaintance);

                // Create appointment
                var appointment = new Appointment
                {
                    DoctorId = DoctorId,
                    PatientId = parsedPatientId,
                    AppointmentTime = AppointmentDateTime,
                    Symptoms = symptom,
                    AcquaintanceId = acquaintance.Id
                };

                // Use transaction
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _appointmentDateService.AddAppointment(appointment);
                        _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(TimeSlotId, false);

                        // Email notifications
                        string body = await _emailService.GetBookingTemplate(appointment.AppointmentTime, doctor.FullName, acquaintance.Name);
                        string remindBody = await _emailService.GetRemindedTemplate(appointment.AppointmentTime, doctor.FullName, acquaintance.Name);
                        BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({patient.FullName})", body));
                        var remindTime = appointment.AppointmentTime.Date.AddDays(-1).AddHours(20);
                        if (remindTime > DateTime.Now)
                        {
                            BackgroundJob.Schedule<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Remind appointment Of ({patient.FullName})", remindBody), remindTime);
                        }

                        // SignalR notifications
                        var updatedDate = appointment.AppointmentTime.Date.ToString("yyyy-MM-dd");
                        await _hubContext.Clients.All.SendAsync("ScheduleUpdated", DoctorId, updatedDate);
                        await _hubContext.Clients.All.SendAsync("TimeSlotBooked", DoctorId, updatedDate, TimeSlotId);
                        await _hubContext.Clients.All.SendAsync("UpdateStatistics");

                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Appointment for acquaintance has been booked successfully!" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = $"Failed to book appointment: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
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

        public async Task<IActionResult> MySchedule(AppointmentStatus? status = null, int page = 1)
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            if (patientId != null)
            {
                var allAppointments = await _appointmentDateService.GetAppointmentsByPatientId(Guid.Parse(patientId), page);

                var filteredAppointments = status.HasValue
                ? await _appointmentDateService.GetAppointmentsByPatientId(Guid.Parse(patientId), status.Value, page)
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

            _appointmentDateService.UpdateStatusAppointment(id, AppointmentStatus.Canceled);
            var timeSlotId = _timeSlotService.GetTimeSlotIdByAppointmentime(appointment.AppointmentTime);
            _timeSlotService.UpdateTimeSlotAvalableStatusByTimeSlotID(timeSlotId, true);
            await _hubContext.Clients.All.SendAsync("UpdateStatus", appointment.AppointmentId, AppointmentStatus.Canceled);

            var updatedDate = appointment.AppointmentTime.Date.ToString("yyyy-MM-dd");
            await _hubContext.Clients.All.SendAsync("ScheduleUpdated", appointment.DoctorId, updatedDate);

            var patient = await _patientService.GetPatientById(appointment.PatientId);

            string body = await _emailService.GetCancelledTemplate(appointment.AppointmentTime, appointment.Doctor.FullName, appointment.Patient.FullName);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress, $"Medical Appointment Of ({appointment.Patient.FullName})", body));

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction("MySchedule");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromForm] FeedbackRequest request)
        {
            await _patientService.AddFeedback(request);
            await _hubContext.Clients.All.SendAsync("UpdateFeedback");
            return Ok();
        }
        [HttpGet]
        [AllowAnonymous]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> GetFeedback(Guid id)
        {
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

        public IActionResult DiagnosisDetail(Guid id)
        {
            var appointment = _appointmentDateService.GetAppointmentsById(id);
            var diagnosisHistory = _appointmentDateService.GetDiagnosisHistoriesByAppointmentID(appointment.AppointmentId);

            ViewData["DiagnosisHistory"] = diagnosisHistory;
            return View(appointment);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Specialities(string searchTerm = null, int page = 1, string filter = "all")
        {
            var specialities = await _specialitiesService.GetAllSpecialitiesAsync(page, searchTerm, filter);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Filter = filter;
            return View(specialities);
        }

        [AllowAnonymous]
        public IActionResult DetailSpecialities(int id)
        {
            var specialities = _specialitiesService.GetSpecialityById(id);
            return View(specialities);
        }

        [HttpPost]
        [Route("Patient/ChatbotMessage")]
        public async Task<IActionResult> ChatbotMessage(string userMessage, string sessionId)
        {
            try
            {
                Console.WriteLine("User message received: " + userMessage);
                Console.WriteLine("Session ID: " + sessionId);

                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                Guid sessionGuid;

                // Nếu sessionId không được cung cấp hoặc không hợp lệ, tạo mới
                if (string.IsNullOrEmpty(sessionId) || !Guid.TryParse(sessionId, out sessionGuid))
                {
                    sessionGuid = Guid.NewGuid();
                }

                var chatRequest = new ChatRequest
                {
                    Query = userMessage,
                    IncludeContext = false,
                    MaxResults = 0,
                    PatientId = patientId != null ? Guid.Parse(patientId) : Guid.Empty,
                    SessionId = sessionGuid
                };

                var chatResponse = await _chatbotService.SendMessageAsync(chatRequest);

                // Return JSON instead of View
                return Json(new { botReply = chatResponse.Response });
            }
            catch (Exception ex)
            {
                // Return error as JSON
                return Json(new { botReply = "Lỗi khi gọi API chatbot: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("Patient/Chatbot")]
        public async Task<IActionResult> Chatbot()
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

            if (patientId != null)
            {
                var patient = await _patientService.GetPatientById(Guid.Parse(patientId));
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientId = patientId;
            }
            else
            {
                ViewBag.PatientName = "Guest";
                ViewBag.PatientId = null;
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChatSessions()
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                Console.WriteLine("Fetching chat sessions for patient ID: " + patientId);

                // Lấy danh sách sessions với tin nhắn đầu tiên của user
                var sessionsWithFirstMessage = await (from session in _context.ChatSessions
                                                      where session.PatientId == Guid.Parse(patientId) && session.IsActive
                                                      select new
                                                      {
                                                          Session = session,
                                                          FirstMessage = _context.ChatMessages
                                                              .Where(msg => msg.SessionId == session.SessionId && msg.IsFromPatient == true)
                                                              .OrderBy(msg => msg.CreatedAt)
                                                              .FirstOrDefault()
                                                      })
                    .Select(x => new
                    {
                        sessionId = x.Session.SessionId.ToString(),
                        sessionName = x.FirstMessage != null ?
                            (x.FirstMessage.MessageText.Length > 50 ?
                                x.FirstMessage.MessageText.Substring(0, 50) + "..." :
                                x.FirstMessage.MessageText) :
                            "Cuộc trò chuyện mới",
                        createdAt = x.Session.CreatedAt,
                        updatedAt = x.Session.UpdatedAt,
                        firstMessageDate = x.FirstMessage != null ? x.FirstMessage.CreatedAt : x.Session.CreatedAt
                    })
                    .OrderByDescending(x => x.updatedAt)
                    .ToListAsync();

                Console.WriteLine("Retrieved chat sessions: " + sessionsWithFirstMessage.Count);

                return Json(sessionsWithFirstMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching chat sessions: " + ex.Message);
                return BadRequest(new { error = "An error occurred while fetching chat sessions" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatMessages(string sessionId)
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                // Kiểm tra session có thuộc về patient này không
                var sessionExists = await _context.ChatSessions
                    .AnyAsync(s => s.SessionId == Guid.Parse(sessionId) &&
                                s.PatientId == Guid.Parse(patientId) &&
                                s.IsActive);

                if (!sessionExists)
                {
                    return BadRequest(new { error = "Session not found or access denied" });
                }

                // Lấy tin nhắn từ database theo sessionId
                var messages = await _context.ChatMessages
                    .Where(m => m.SessionId == Guid.Parse(sessionId))
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new
                    {
                        MessageText = m.MessageText,
                        IsFromPatient = m.IsFromPatient,
                        CreatedAt = m.CreatedAt
                    })
                    .ToListAsync();

                return Json(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChatSession([FromBody] CreateChatSessionRequest request)
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                var session = new ChatSessions
                {
                    SessionId = Guid.Parse(request.SessionId),
                    PatientId = Guid.Parse(patientId),
                    SessionName = request.SessionName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };

                Console.WriteLine("Session: " + session.SessionId + " - " + session.SessionName + " - " + session.CreatedAt);

                _context.ChatSessions.Add(session);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    sessionId = session.SessionId.ToString(),
                    sessionName = session.SessionName,
                    createdAt = session.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChatSessionName([FromBody] UpdateSessionNameRequest request)
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                var session = await _context.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == Guid.Parse(request.SessionId) &&
                                            s.PatientId == Guid.Parse(patientId) &&
                                            s.IsActive);

                if (session != null)
                {
                    session.SessionName = request.SessionName;
                    session.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest(new { error = "Session not found or access denied" });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChatSession([FromBody] DeleteSessionRequest request)
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                Console.WriteLine("Deleting session with ID: " + patientId);

                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                var session = await _context.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == request.SessionId &&
                                            s.PatientId == Guid.Parse(patientId) &&
                                            s.IsActive);

                Console.WriteLine($"Deleting session: {session?.SessionId} for patient: {patientId}");

                if (session != null)
                {
                    _context.ChatMessages.RemoveRange(
                        _context.ChatMessages.Where(m => m.SessionId == session.SessionId));
                    _context.ChatSessions.Remove(session);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest(new { error = "Session not found or access denied" });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveChatMessage([FromBody] SaveMessageRequest request)
        {
            try
            {
                var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");

                if (string.IsNullOrEmpty(patientId))
                {
                    return BadRequest(new { error = "Patient not authenticated" });
                }

                // Kiểm tra session có thuộc về patient này không
                var sessionExists = await _context.ChatSessions
                    .AnyAsync(s => s.SessionId == Guid.Parse(request.SessionId) &&
                                s.PatientId == Guid.Parse(patientId) &&
                                s.IsActive);

                if (!sessionExists)
                {
                    return BadRequest(new { error = "Session not found or access denied" });
                }

                var message = new ChatMessages
                {
                    SessionId = Guid.Parse(request.SessionId),
                    MessageText = request.MessageText,
                    IsFromPatient = request.IsFromPatient,
                    CreatedAt = DateTime.Now
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                // Cập nhật UpdatedAt của session để sắp xếp đúng thứ tự
                var session = await _context.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == Guid.Parse(request.SessionId));
                if (session != null)
                {
                    session.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet]
        [Route("Patient/PersonalDisease")]
        public IActionResult PersonalDisease(int page = 1, int pageSize = 10)
        {
            var patientId = _contextAccessor.HttpContext?.Session.GetString("PatientId");
            if (string.IsNullOrEmpty(patientId))
            {
                return RedirectToAction("Index", "Patient");
            }

            // Lấy danh sách bệnh án từ service
            var diagnosisHistory = _patientService.GetDiagnosisHistoriesByPatientId(Guid.Parse(patientId));

            // Phân trang
            var pagedDiagnoses = diagnosisHistory
                .OrderByDescending(d => d.Appointment.AppointmentTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = diagnosisHistory.Count;

            var pagination = new Pagination<DiagnosisHistory>(
                pagedDiagnoses,
                totalCount,
                page,
                pageSize
            );

            return View(pagination);
        }
    }
}
