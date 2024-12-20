using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppointmentHospital.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService doctorService;
        private readonly ILogger<DoctorController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAppointmentDateService _appointmentDateService;
        
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService, IHttpContextAccessor contextAccessor, IAppointmentDateService appointmentDateService)
        {
            _logger = logger;
            this.doctorService = doctorService;
            this._contextAccessor = contextAccessor;
            this._appointmentDateService = appointmentDateService;
        }

        public IActionResult Index()
        {
            var doctorId = _contextAccessor.HttpContext?.Session.GetString("DoctorId");
            var appointments = _appointmentDateService.GetAppointmentsByDoctorId(Guid.Parse(doctorId));
            return View(appointments); 
        }
    }
}
