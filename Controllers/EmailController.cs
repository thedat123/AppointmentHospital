using AppointmentHospital.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentHospital.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public EmailController(IEmailService emailService, IBackgroundJobClient backgroundJobClient) 
        {
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendMail(DateTime appointmentTime, string doctorName, string fullUserName, string toEmail)
        {
            var contentBody = await _emailService.GetBookingTemplate(appointmentTime, doctorName, fullUserName);
            BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendMailAsync(toEmail, "Email booking", contentBody));
            var remindDate = appointmentTime.AddDays(-1).Date.AddHours(20);
            if(DateTime.Now < remindDate)
            {
                BackgroundJob.Schedule<IEmailService>(emailService => emailService.SendMailAsync(toEmail, "Reminded booking", contentBody), remindDate);
            }
            var jobId = BackgroundJob.Schedule<IEmailService>( emailService => emailService.SendMailAsync(toEmail, "Đây là email huỷ lịch hẹn" , contentBody), TimeSpan.FromSeconds(15));
            return RedirectToAction("Index");
        }
    }
}
