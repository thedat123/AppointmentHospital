using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System;
using System.Linq.Expressions;

namespace AppointmentHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManagingDoctorController : Controller
    {
        private readonly IManagingDoctorService _managingDoctorService;
        private readonly ITimeSlotService _timeSlotService;
        public ManagingDoctorController(IManagingDoctorService managingDoctorService, ITimeSlotService timeSlotService)
        {
            _managingDoctorService = managingDoctorService;
            _timeSlotService = timeSlotService;
        }
        public async Task<IActionResult> Index(int? page, string searchTerm, int SpecialityId)
        {
            ViewData["SelectSpecialization"] = _managingDoctorService.GetSpecialization();
            ViewData["Specialization"] = SpecialityId;
            ViewBag.SearchTerm = searchTerm;
            
            // Nếu không có searchTerm hoặc SpecialityId thì load full data
            var doctorList = await _managingDoctorService.GetAllDoctor(
                page ?? 1, 
                string.IsNullOrEmpty(searchTerm) ? null : searchTerm, 
                SpecialityId
            );
            
            return View(doctorList);
        }
        public IActionResult CreateDoctor()
        {
            ViewData["SelectSpecialization"] = _managingDoctorService.GetSpecialization();
            return View(new ManagingDoctorRequest());
        }
        [HttpPost]
        public async Task<IActionResult> CreateDoctor(ManagingDoctorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            await _managingDoctorService.CreateNewDoctorAsync(request);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var timeSlots = await _managingDoctorService.GetTimeSlotByDoctorIdAsync(id);

            if (timeSlots)
            {
                return Json(new { success = false, message = "Không thể xóa bác sĩ này vì đã có lịch hẹn được đăng ký." });
            }

            try
            {
                await _managingDoctorService.DeleteDoctorAsync(id);
                return Json(new { success = true, message = "Xóa bác sĩ thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa bác sĩ: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(Guid id)
        {
            var doctor = await _managingDoctorService.GetDoctorAsync(id);
            ViewData["SpecializationList"] = _managingDoctorService.GetSpecialization();
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> EditDoctor(Guid id, ManagingDoctorRequest request)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Editing doctor with ID: {id}");
                Console.WriteLine("Model state is invalid.");
                
                // Debug: In ra các lỗi validation
                foreach (var modelError in ModelState)
                {
                    var key = modelError.Key;
                    var errors = modelError.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                
                ViewData["SpecializationList"] = _managingDoctorService.GetSpecialization();
                
                // Chuyển đổi request thành response để hiển thị lại trong view
                var responseModel = new ManagingDoctorResponse
                {
                    Id = id,
                    FullName = request.FullName,
                    EmailAddress = request.EmailAddress,
                    Degree = request.Degree,
                    SpecialityId = request.SpecialityId,
                    ImagePath = request.ImagePath,
                    Introduction = request.Introduction,
                    Awards = request.Awards,
                    Expertise = request.Expertise,
                    OrganizationMember = request.OrganizationMember,
                    ResearchProject = request.ResearchProject,
                    TrainingProcess = request.TrainingProcess,
                    WorkExperience = request.WorkExperience
                };
                
                return View(responseModel);
            }
            
            Console.WriteLine($"Editing doctor with ID: {id}");
            await _managingDoctorService.EditDoctorAsync(id, request);
            return RedirectToAction("Index");
        }
    }
}
