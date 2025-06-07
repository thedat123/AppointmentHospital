using AppointmentHospital.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentHospital.Areas.Admin.DTOs.ManagingPatient;

namespace AppointmentHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class ManagingPatientController : Controller
    {
        private readonly IManagingPatientService _managingPatientService;
        public ManagingPatientController(IManagingPatientService managingPatientService)
        {
            _managingPatientService = managingPatientService;
        }
        public async Task<IActionResult> Index(int? page, string? searchTerm)
        {
            var patientList = await _managingPatientService.GetAllPatientAsync(page ?? 1, searchTerm);
            ViewBag.PatientList = patientList;
            ViewBag.SearchTerm = searchTerm;
            return View(patientList);
        }
        [HttpGet]
        public IActionResult CreatePatient()
        {
            return View(new ManagingPatientRequest());
        }
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromForm]ManagingPatientRequest request)
        {
            if(!ModelState.IsValid)
            {
                return View(request);
            }
            var resultCreate =  await _managingPatientService.CreateNewPatientAsync(request);
            if(!resultCreate)
            {
                return View(request);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeletePatient(Guid id, int? page)
        {
            await _managingPatientService.DeletePatientAsync(id);
            return RedirectToAction("Index",page ?? 1);
        }
        [HttpGet]
        public async Task<IActionResult> EditPatient(Guid id)
        {
            var patient = await _managingPatientService.GetPatientAsync(id);
            return View(patient);
        }
        [HttpPost]
        public async Task<IActionResult> EditPatient(Guid id, ManagingPatientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            await _managingPatientService.EditPatientAsync(id, request);
            return RedirectToAction("Index");
        }
    }
}
