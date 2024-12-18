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
            var contentBody = await _emailService.GetCancelledTemplate(appointmentTime, doctorName, fullUserName);
            var jobId = BackgroundJob.Schedule<IEmailService>( emailService => emailService.SendMailAsync(toEmail, "Đây là email huỷ lịch hẹn" , contentBody), TimeSpan.FromSeconds(15));
            _backgroundJobClient.ContinueJobWith(jobId, () => Console.WriteLine($" Task {jobId} has finished"));
            return RedirectToAction("Index");
        }
    }
}
