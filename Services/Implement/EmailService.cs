
using AppointmentHospital.Configuration.EmailConfiguaration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AppointmentHospital.Services.Implement
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<EmailConfiguration> _options;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmailService(IOptions<EmailConfiguration> options, IWebHostEnvironment webHostEnvironment)
        {
            _options = options;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GetBookingTemplate(DateTime appointmentTime, string doctorName, string fullUserName)
        {
            string content = await System.IO.File.ReadAllTextAsync(Path.Combine(_webHostEnvironment.WebRootPath, "assets", "template", "mails", "bookingTemplate.html"));
            content = content.Replace("{{FullUserName}}", fullUserName);
            content = content.Replace("{{AppointmentTime}}", appointmentTime.ToString("dd/MM/yyyy HH:mm"));
            content = content.Replace("{{DoctorName}}", doctorName);
            return content;
        }

        public async Task<string> GetCancelledTemplate(DateTime appointmentTime, string doctorName, string fullUserName)
        {
            string content = await System.IO.File.ReadAllTextAsync(Path.Combine(_webHostEnvironment.WebRootPath, "assets", "template", "mails", "cancelledTemplate.html"));
            content = content.Replace("{{fullUserName}}", fullUserName);
            content = content.Replace("{{appointmentTime}}", appointmentTime.ToString("dd/MM/yyyy HH:mm"));
            content = content.Replace("{{doctorName}}", doctorName);
            return content;
        }

        public async Task<string> GetConfirmedEmailTemplate(string fullUserName, string url)
        {
             var content = await System.IO.File.ReadAllTextAsync(Path.Combine(_webHostEnvironment.WebRootPath, "assets", "template", "mails", "confirmedEmail.html"));
             content = content.Replace("{{fullUserName}}", fullUserName);
             content = content.Replace("{{confirmationLink}}", url);
             return content;
        }

        public async Task<string> GetRemindedTemplate(DateTime appointmentTime, string doctorName, string fullUserName)
        {
            string content = await System.IO.File.ReadAllTextAsync(Path.Combine(_webHostEnvironment.WebRootPath, "assets", "template", "mails", "remindedTemplate.html"));
            content = content.Replace("{{fullUserName}}", fullUserName);
            content = content.Replace("{{appointmentTime}}", appointmentTime.ToString("dd/MM/yyyy HH:mm"));
            content = content.Replace("{{doctorName}}", doctorName);
            return content;
        }

        public async Task<string> GetResetPasswordTemplate(string fullUserName, string url)
        {
            var content = await System.IO.File.ReadAllTextAsync(Path.Combine(_webHostEnvironment.WebRootPath, "assets", "template", "mails", "resetPasswordTemplate.html"));
            content = content.Replace("{{fullUserName}}", fullUserName);
            content = content.Replace("{{resetPasswordLink}}", url);
            return content;
        }

        public async Task SendMailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var port = _options.Value.Port;
                var host = _options.Value.Host;
                var username = _options.Value.UserName;
                var password = _options.Value.Password;
                var enableSSL = _options.Value.EnableSSL;

                MailMessage message = new MailMessage(new MailAddress(username).ToString(), new MailAddress(toEmail).ToString(), subject, body);
                message.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Port = port;
                smtpClient.Host = host;
                smtpClient.Credentials = new NetworkCredential(username, password);
                smtpClient.EnableSsl = enableSSL;
                await smtpClient.SendMailAsync(message);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
