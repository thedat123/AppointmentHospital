using AppointmentHospital.Areas.Admin.DTOs.ManagingDoctor;
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.EnumStatus;
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
        public ManagingDoctorController(IManagingDoctorService managingDoctorService)
        {
            _managingDoctorService = managingDoctorService;
        }
        public async Task<IActionResult> Index(int? page, string searchTerm, Specialization? specialization)
        {
            ViewData["SelectSpecialization"] = _managingDoctorService.GetSpecialization();
            ViewData["Specialization"] = specialization;
            ViewBag.SearchTerm = searchTerm;
            var doctorList = await _managingDoctorService.GetAllDoctor(page ?? 1, searchTerm, specialization);
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
            await _managingDoctorService.DeleteDoctorAsync(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditDoctor(Guid id)
        {
            var doctor = await _managingDoctorService.GetDoctorAsync(id);
            ViewData["SpecializationList"] = _managingDoctorService.GetSpecialization();
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> EditDoctor(Guid id, ManagingDoctorRequest request)
        {
            await _managingDoctorService.EditDoctorAsync(id, request);
            return RedirectToAction("Index");
        }
    }
}
