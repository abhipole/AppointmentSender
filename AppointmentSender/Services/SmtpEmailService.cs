using AppointmentSender.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO;

namespace AppointmentSender.Services
{
    public class SmtpEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _logger = logger;
            _settings = config.GetSection("Smtp").Get<EmailSettings>();
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, byte[] attachmentBytes = null, string attachmentName = "AppointmentLetter.txt")
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.Username, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            if (attachmentBytes != null)
            {
                message.Attachments.Add(new Attachment(new MemoryStream(attachmentBytes), attachmentName));
            }

            using var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.UseSsl
            };

            try
            {
                _logger.LogInformation($"Sending email to {to} via {_settings.Host}:{_settings.Port}");
                await smtp.SendMailAsync(message);
                _logger.LogInformation($"Email sent to {to} successfully.");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error while sending email to {to}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while sending email to {to}");
                throw;
            }
        }
    }
}
