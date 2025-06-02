using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.EnumStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using AppointmentHospital.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class SpecialityController : Controller
    {
        private readonly ISpecialityService _specialityService;

        public SpecialityController(ISpecialityService specialityService)
        {
            _specialityService = specialityService;
        }

        public async Task<IActionResult> SpecialityIndex(int? page, string searchTerm)
        {
            var specialities = await _specialityService.GetAllSpeciality(page ?? 1, searchTerm);
            return View(specialities);
        }

        public IActionResult CreateSpeciality()
        {
            return View(new Specialities());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpeciality(Specialities speciality)
        {
            if (!ModelState.IsValid)
            {
                return View(speciality);
            }

            await _specialityService.CreateSpecialityAsync(speciality);
            return RedirectToAction("SpecialityIndex");
        }

        public async Task<IActionResult> DeleteSpeciality(int id)
        {
            var result = await _specialityService.DeleteSpecialityAsync(id);
    
            if (!result)
            {
                TempData["ErrorMessage"] = "Không thể xóa chuyên khoa này vì còn có bác sĩ đang làm việc trong chuyên khoa.";
            }
            else
            {
                TempData["SuccessMessage"] = "Xóa chuyên khoa thành công!";
            }
            return RedirectToAction("SpecialityIndex");
        }

        [HttpGet]
        public async Task<IActionResult> EditSpeciality(int id)
        {
            var speciality = await _specialityService.GetSpecialityAsync(id);
            return View(speciality);
        }

        [HttpPost]
        public async Task<IActionResult> EditSpeciality(int id, Specialities speciality)
        {
            if (!ModelState.IsValid)
            {
                // Debug what's invalid
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(speciality);
            }
            Console.WriteLine($"Editing speciality with ID: {id}");
            await _specialityService.EditSpecialityAsync(id, speciality);
            return RedirectToAction("SpecialityIndex");
        }
    }
}