using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.Areas.Admin.Services.Implement;
using AppointmentHospital.EnumStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        public async Task<IActionResult> Index(int? page, string? searchTerm, Specialization? specialization )
        {
            // kẹp giá trị của Specialization hiện tại vào
            ViewData["StatusList"] = _appointmentService.GetStatus();
            ViewData["SpecializationList"] = _appointmentService.GetSpecialization();
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Specialization"] = specialization;
            var appointmentList = await _appointmentService.GetAllAppointmentAsync(page ?? 1, searchTerm, specialization);
            return View(appointmentList);
        }
        public async Task<IActionResult> Detail(Guid id)
        {
            var appointment = await _appointmentService.GetAppointmentAsync(id);
            return View(appointment);
        }
    }
}
