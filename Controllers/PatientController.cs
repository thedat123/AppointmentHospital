using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDbContext appDbContext;
        private readonly IDoctorService doctorService;
        private readonly ILogger<DoctorController> _logger;
        public PatientController(AppDbContext appDbContext, IDoctorService doctorService, ILogger<DoctorController> logger){
            this.appDbContext = appDbContext;
            this.doctorService = doctorService;
            this._logger = logger;
        }

        public IActionResult Index()
        {
            List<Doctor> doctors = appDbContext.Doctors.ToList(); 
            return View(doctors);
        }

        [HttpPost]
        public IActionResult BookSchedule(Guid DoctorId, String selectedDate, String selectedTime){
            DateTime appointmentDateTime = DateTime.Parse($"{selectedDate} {selectedTime}");
            String DoctorName = doctorService.getDoctorNameByDoctorId(DoctorId);
            var model = new
            {
                DoctorName = DoctorName,
                AppointmentDateTime = appointmentDateTime
            };

            return View(model);
        }

        public IActionResult DetailDoctor(Guid id)
        {
            Doctor doctor = doctorService.getDoctorById(id);
            List<TimeSlot> timeSlots = doctorService.getTimeSlotByDoctorId(id);

            var viewModel = new DoctorDetailViewModel
            {
                Doctor = doctor,
                TimeSlots = timeSlots
            };

            return View(viewModel);
        }
    }
}
