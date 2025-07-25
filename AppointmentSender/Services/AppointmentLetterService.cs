using AppointmentSender.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using System;
using System.Threading.Tasks;

namespace AppointmentSender.Services
{
    public class AppointmentLetterService
    {
        private readonly SmtpEmailService _emailService;
        private readonly ILogger<AppointmentLetterService> _logger;

        public AppointmentLetterService(
            SmtpEmailService emailService,
            ILogger<AppointmentLetterService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateAppointmentLetterPdfAsync(Employee employee)
        {
            try
            {
                var document = new AppointmentLetterDocument(employee);
                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PDF generation failed for {employee.Email}");
                throw;
            }
        }

        public async Task SendEmailWithPdfAsync(Employee employee, byte[] pdfBytes)
        {
            var subject = "Your Appointment Letter (PDF)";
            var htmlBody = $@"
                <p>Dear {employee.FirstName},</p>
                <p>Please find your appointment letter attached.</p>
                <p>Regards,<br/>HR Department</p>";

            try
            {
                await _emailService.SendEmailAsync(
                    to: employee.Email,
                    subject: subject,
                    htmlBody: htmlBody,
                    attachmentBytes: pdfBytes,
                    attachmentName: "AppointmentLetter.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email sending failed to {employee.Email}");
                throw;
            }
        }
    }
}
