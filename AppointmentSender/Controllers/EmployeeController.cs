using AppointmentSender.Data;
using AppointmentSender.Models;
using AppointmentSender.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;

namespace AppointmentSender.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AppointmentLetterService _letterService;

        public EmployeeController(AppDbContext context, AppointmentLetterService letterService)
        {
            _context = context;
            _letterService = letterService;
        }

        // GET: /Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        // GET: /Employee/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    var pdf = await _letterService.GenerateAppointmentLetterPdfAsync(employee);
                    await _letterService.SendEmailWithPdfAsync(employee, pdf);

                    TempData["Message"] = $"Appointment letter sent to {employee.Email}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(employee);
        }

        // GET: /Employee/UploadExcel
        [HttpGet]
        public IActionResult UploadExcel()
        {
            return View();
        }

        // POST: /Employee/UploadExcel
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid Excel file.");
                return View();
            }

            int successCount = 0;
            int failCount = 0;

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0; // ✅ Reset stream position

                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);

                for (int row = 1; row <= sheet.LastRowNum; row++) // 0-based index, row 1 = Excel Row 2
                {
                    var currentRow = sheet.GetRow(row);
                    if (currentRow == null) continue;

                    try
                    {
                        var employee = new Employee
                        {
                            FirstName = currentRow.GetCell(0)?.ToString()?.Trim(),
                            Email = currentRow.GetCell(1)?.ToString()?.Trim(),
                            CTC = decimal.TryParse(currentRow.GetCell(2)?.ToString(), out var ctcVal) ? ctcVal : 0,
                            Breakdown = currentRow.GetCell(3)?.ToString()?.Trim()
                        };

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        var pdf = await _letterService.GenerateAppointmentLetterPdfAsync(employee);
                        await _letterService.SendEmailWithPdfAsync(employee, pdf);

                        successCount++;
                    }
                    catch
                    {
                        failCount++;
                    }
                }
            }

            TempData["Message"] = $"Uploaded: {successCount} ✔️ | Failed: {failCount} ❌";
            return RedirectToAction(nameof(Index));
        }
    }
}
