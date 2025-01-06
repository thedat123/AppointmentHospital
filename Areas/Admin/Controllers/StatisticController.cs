using AppointmentHospital.Areas.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class StatisticController : Controller
    {
        private readonly IStatisticService _statisticService;
        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }
        public IActionResult Index()
        {
            ViewBag.Months =  _statisticService.GetMonthsInYear();
            ViewBag.Weeks  =  _statisticService.GetWeeksAllMonth(2024);
            return View();
        }
        [HttpGet]
        public IActionResult GetWeeksByYear(int year) {
            var weeks = _statisticService.GetWeeksAllMonth(year);
            return Json(weeks);
        }

        [HttpGet]
        public async Task<IActionResult> Statistic([FromQuery]DateTime? singleDate, [FromQuery] int? month, [FromQuery] int? week, [FromQuery] int? year)
        {
            var oldAndNewUser = await _statisticService.GetNewAndOldUser(singleDate, month, week, year);
            var amountAppointment = await _statisticService.GetAmountAppointment(singleDate, month, week, year);
            var topDoctorAppointment =  await _statisticService.GetTopDoctorAppointment(singleDate, month, week, year);
            var compareAmountAppointment = await _statisticService.GetCompareAmountAppointment(singleDate, month, week, year);
            var compareAmountOldAndNewUser = await _statisticService.GetCompareNewAndOldUser(singleDate, month, week, year);
            return new JsonResult(new { oldAndNewUser = oldAndNewUser,
                                        amountAppointment = amountAppointment,
                                        topDoctorAppointment = topDoctorAppointment,
                                        compareAmountAppointment = compareAmountAppointment,
                                        compareAmountOldAndNewUser = compareAmountOldAndNewUser });
        }
    }
}
