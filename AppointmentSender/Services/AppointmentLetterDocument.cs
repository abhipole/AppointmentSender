using AppointmentSender.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AppointmentSender.Services
{
    public class AppointmentLetterDocument : IDocument
    {
        private readonly Employee _employee;

        public AppointmentLetterDocument(Employee employee)
        {
            _employee = employee;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Appointment Letter").FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                    col.Item().Text($"Dear {_employee.FirstName},").FontSize(14);
                    col.Item().Text($"We are pleased to offer you the position with a CTC of ₹{_employee.CTC:N2}.").FontSize(12);
                    col.Item().Text("CTC Breakdown:").FontSize(12).Bold();
                    col.Item().Text(_employee.Breakdown).FontSize(12);
                    col.Item().PaddingVertical(10).Text("We look forward to having you on our team.").FontSize(12);
                    col.Item().Text("Regards,").FontSize(12);
                    col.Item().Text("HR Department").FontSize(12);
                });
            });
        }
    }
}
