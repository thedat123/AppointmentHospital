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
        
        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService)
        {
            _logger = logger;
            this.doctorService = doctorService;
        }

        public IActionResult Index()
        {
            List<Doctor> doctor = doctorService.getAllDoctors();
            return View(doctor);
        }

    }
}
