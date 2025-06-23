using System.Security.Claims;
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using AppointmentHospital.Services.Implement;
using AppointmentHospital.ViewModels;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace AppointmentHospital.Controllers
{
    [Authorize(Roles = "Doctor")]
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
        private readonly CloudinaryService _cloudinaryService;
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService, ITimeSlotService timeSlotService, IEmailService emailService, IPatientService patientService, IManagingDoctorService managingDoctorService, IHubContext<ScheduleHub> hubContext, CloudinaryService cloudinaryService)
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
            this._cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> Index(AppointmentStatus? status = AppointmentStatus.Confirmed, int page = 1)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");

            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("Doctor ID is missing.");
            }

            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var stats = new
            {
                PendingCount = _appointmentDateService.CountAppointmentDoctorIdStatus(Guid.Parse(doctorId), AppointmentStatus.Pending),
                ConfirmedCount = _appointmentDateService.CountAppointmentDoctorIdStatus(Guid.Parse(doctorId), AppointmentStatus.Confirmed),
                CompletedCount = _appointmentDateService.CountAppointmentDoctorIdStatus(Guid.Parse(doctorId), AppointmentStatus.Completed),
                CanceledCount = _appointmentDateService.CountAppointmentDoctorIdStatus(Guid.Parse(doctorId), AppointmentStatus.Canceled)
            };

            var filteredAppointments = status.HasValue
                ? await _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId), status.Value, page)
                : await _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId), page);

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

        public async Task<IActionResult> Calendar(DateTime? filterDate, int page = 1, string sortBy = "Days", string sortOrder = "asc")
        {
            if (string.IsNullOrEmpty(sortBy))
                sortBy = "Days";
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "asc";

            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
            ViewBag.DoctorName = doctor?.FullName ?? "Unknown Doctor";
            ViewBag.Speciality = doctor?.Specialities?.SpecialityName ?? "Unknown Speciality";
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.FilterDate = filterDate?.ToString("yyyy-MM-dd");

            var timeSlots = await _timeSlotService.GetTimeSlotByDoctorId(Guid.Parse(doctorId), page, sortBy, sortOrder, filterDate);
            return View(timeSlots);
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
            var parsedDoctorId = Guid.Parse(doctorId);
            int addedSlots = 0;

            foreach (var day in remainingDays)
            {
                var schedule = input.Schedules.FirstOrDefault(s =>
                    s.DayOfWeek == day.DayOfWeek &&
                    s.StartTime.HasValue &&
                    s.EndTime.HasValue);

                if (schedule == null) continue;

                for (TimeSpan currentStartTime = schedule.StartTime.Value;
                    currentStartTime < schedule.EndTime.Value;
                    currentStartTime = currentStartTime.Add(TimeSpan.FromHours(1)))
                {
                    var newTimeSlot = new TimeSlot
                    {
                        TimeSlotId = Guid.NewGuid(),
                        DoctorId = parsedDoctorId,
                        StartTime = day.Date.Add(currentStartTime),
                        EndTime = day.Date.Add(currentStartTime.Add(TimeSpan.FromHours(1))),
                        Available = true
                    };

                    if (_timeSlotService.AddTimeSlot(newTimeSlot))
                    {
                        addedSlots++;
                    }
                }
            }

            TempData["SuccessMessage"] = addedSlots > 0 
                ? $"Update successful! Added {addedSlots} new time slots."
                : "No new time slots added due to existing schedules.";
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
            if (User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var doctor = doctorService.getDoctorById(Guid.Parse(doctorId));
                ViewBag.Specializaiton = _managingDoctorService.GetSpecialization();
                ViewBag.DoctorId = doctorId;
                return View(doctor);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(DoctorInfoUpdate request, string phoneNumber, IFormFile profileImage, bool removeImage = false)
        {
            try
            {
                ModelState.Remove("profileImage");
                var doctorUpdate = doctorService.getDoctorById(request.DoctorId);
                string imageUrl = null;

                // Xử lý xóa ảnh
                if (removeImage)
                {
                    if (doctorUpdate != null && !string.IsNullOrEmpty(doctorUpdate.ImagePath))
                    {
                        // Xóa ảnh trên Cloudinary (nếu cần)
                        var publicId = Path.GetFileNameWithoutExtension(doctorUpdate.ImagePath); // Lấy public ID từ URL
                        await _cloudinaryService.DeleteImageAsync(publicId);
                        imageUrl = null; // Đặt imageUrl thành null để xóa ImagePath
                    }
                }
                // Xử lý tải lên ảnh mới
                else if (profileImage != null && profileImage.Length > 0)
                {
                    using (var stream = profileImage.OpenReadStream())
                    {
                        imageUrl = await _cloudinaryService.UploadImageAsync(stream, profileImage.FileName);
                    }
                }

                // Cập nhật thông tin bác sĩ
                var doctor = await doctorService.UpdateDoctor(request, phoneNumber, imageUrl);
                ViewBag.Specializaiton = _managingDoctorService.GetSpecialization();
                ViewBag.DoctorId = doctor.DoctorId;

                await _hubContext.Clients.All.SendAsync("UpdateDoctorProfile", doctor);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return View("PersonalInfo", doctor);
            }
            catch (Exception ex)
            {
                ViewBag.Specializaiton = _managingDoctorService.GetSpecialization();
                ViewBag.DoctorId = request.DoctorId;
                ModelState.AddModelError("", ex.Message);
                var doctor = doctorService.getDoctorById(request.DoctorId);
                return View("PersonalInfo", doctor);
            }
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
        public async Task<IActionResult> SubmitDiagnosis(Guid AppointmentId, Guid PatientId, Guid DoctorId, Guid AcquaintanceId, DateTime DateTime, string DiagnosisDetails, string PrescribedMedications, string DoctorNotes)
        {
            List<string> prescribedMedicationList = PrescribedMedications?.Split(',').ToList() ?? new List<string>();
            DiagnosisHistory diagnosisHistory = new DiagnosisHistory
            {
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
            var acquaintance = AcquaintanceId != null ? await _patientService.GetAcquaintanceById(AcquaintanceId) : null;

            string body = await _emailService.GetCompletedTemplate(acquaintance != null ? acquaintance.Name : patient.FullName, doctorService.getDoctorById(DoctorId).FullName, DiagnosisDetails, PrescribedMedicationsHtml, DoctorNotes);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(patient.EmailAddress,$"Medical Appointment Of ({patient.FullName})", body));

            _appointmentDateService.UpdateStatusAppointment(AppointmentId, AppointmentStatus.Completed);
            await _hubContext.Clients.All.SendAsync("UpdateStatus", AppointmentId, AppointmentStatus.Completed);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteTimeSlot(Guid id)
        {
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
                    TempData["timeSlotIds"] = JsonConvert.SerializeObject(notifyPatientSlots);
                    TempData["offDate"] = offDate.ToString("yyyy-MM-dd");
                    TempData["note"] = Note;
                    return Json(new
                    {
                        success = false,
                        showSuggestModal = true,
                        timeSlotIds = JsonConvert.SerializeObject(notifyPatientSlots),
                        offDate = offDate.ToString("yyyy-MM-dd"),
                        note = Note
                    });
                }

                return Json(new { success = true, message = "Đăng ký ngày nghỉ thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Bạn không có lịch làm việc vào ngày này" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SuggestDayForMultiplePatients(DateTime suggestDate, string timeSlotIds, string offDate, string note)
        {
            if (string.IsNullOrEmpty(timeSlotIds) || string.IsNullOrEmpty(offDate))
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Calendar");
            }

            var timeSlotIdList = JsonConvert.DeserializeObject<List<Guid>>(timeSlotIds);
            var offDateParsed = DateTime.Parse(offDate);
            var doctorId = Guid.Parse(_contextAccessor.HttpContext?.Session.GetString("DoctorId")!);

            foreach (var timeSlotId in timeSlotIdList)
            {
                var timeSlot = _timeSlotService.GetTimeSlotById(timeSlotId);
                if (timeSlot == null) continue;

                var appointments = _appointmentDateService.GetAppointmentsByDoctorIdAndDate(doctorId, offDateParsed);
                foreach (var appointment in appointments)
                {
                    await ProcessAppointmentAsync(appointment, suggestDate);
                }

                _timeSlotService.UpdateTimeSlot(timeSlotId, suggestDate);
            }

            TempData["SuccessMessage"] = "Đề xuất ngày khám mới đã được gửi thành công!";
            return RedirectToAction("Calendar");
        }

        private async Task ProcessAppointmentAsync(Appointment appointment, DateTime suggestDate)
        {
            var patient = await _patientService.GetPatientById(appointment.PatientId);
            if (patient == null) return;

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Canceled)
            {
                return;
            }

            _appointmentDateService.UpdateStatusAppointment(appointment.AppointmentId, AppointmentStatus.Canceled);

            string emailBody = await _emailService.GetCancelAndSuggestTemplate(
                appointment.AppointmentTime,
                appointment.Doctor.FullName,
                appointment.Acquaintance?.Name ?? patient.FullName,
                suggestDate
            );

            BackgroundJob.Enqueue<IEmailService>(emailService =>
                emailService.SendMailAsync(
                    patient.EmailAddress,
                    $"Hủy và đề xuất lịch hẹn mới cho {patient.FullName}",
                    emailBody
                ));
        }

        [HttpPost]
        public async Task<IActionResult> SuggestDay(Guid timeSlotId, DateTime suggestDate)
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var timeSlot = _timeSlotService.GetTimeSlotById(timeSlotId);

            if (timeSlot == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction("Calendar");
            }

            var appointment = _appointmentDateService.GetAppointmentsByDoctorIdAndStartTime(Guid.Parse(doctorId), timeSlot.StartTime);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy cuộc hẹn liên quan.";
                return RedirectToAction("Calendar");
            }

            var patient = await _patientService.GetPatientById(appointment.PatientId);
            if (patient != null)
            {
                _appointmentDateService.UpdateStatusAppointment(appointment.AppointmentId, AppointmentStatus.Canceled);
                _timeSlotService.DeleteTimeSlot(timeSlot.TimeSlotId);

                string body = await _emailService.GetCancelAndSuggestTemplate(
                    appointment.AppointmentTime,
                    appointment.Doctor.FullName,
                    appointment.Acquaintance?.Name ?? patient.FullName,
                    suggestDate
                );
                BackgroundJob.Enqueue<IEmailService>(emailService =>
                    emailService.SendMailAsync(
                        patient.EmailAddress,
                        $"Medical Appointment Of ({patient.FullName})",
                        body
                    ));
            }

            TempData["SuccessMessage"] = "Đề xuất ngày khám mới đã được gửi thành công!";
            return RedirectToAction("Calendar");
        }
    }
}