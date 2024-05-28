using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using MedicalOffice.ViewModels;
using MedicalOffice.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class ReportController : CognizantController
    {
        private readonly MedicalOfficeContext _context;

        public ReportController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // Displays the index page for reports
        public IActionResult Index()
        {
            return View();
        }

        // Generates an Excel file with appointment details and downloads it
        public IActionResult DownloadAppointments()
        {
            var appts = from a in _context.Appointments
                        .Include(a => a.AppointmentReason)
                        .Include(a => a.Patient)
                        .ThenInclude(p => p.Doctor)
                        orderby a.StartTime descending
                        select new
                        {
                            Date = a.StartTime.ToShortDateString(),
                            Patient = a.Patient.FullName,
                            Reason = a.AppointmentReason.ReasonName,
                            Fee = a.ExtraFee,
                            Phone = a.Patient.PhoneFormatted,
                            Doctor = a.Patient.Doctor.FullName,
                            a.Notes
                        };
            int numRows = appts.Count();

            if (numRows > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    var workSheet = excel.Workbook.Worksheets.Add("Appointments");

                    workSheet.Cells[3, 1].LoadFromCollection(appts, true);

                    workSheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd";
                    workSheet.Column(4).Style.Numberformat.Format = "###,##0.00";
                    workSheet.Cells[4, 1, numRows + 3, 2].Style.Font.Bold = true;

                    using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 4])
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 4].Address + ":" + workSheet.Cells[numRows + 3, 4].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "$###,##0.00";
                    }

                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }

                    for (int i = 4; i < numRows + 4; i++)
                    {
                        using (ExcelRange Rng = workSheet.Cells[i, 7])
                        {
                            string[] commentWords = Rng.Value.ToString().Split(' ');
                            Rng.Value = commentWords[0] + "...";
                            string comment = string.Join(Environment.NewLine, commentWords
                                .Select((word, index) => new { word, index })
                                .GroupBy(x => x.index / 7)
                                .Select(grp => string.Join(" ", grp.Select(x => x.word))));
                            ExcelComment cmd = Rng.AddComment(comment, "Apt. Notes");
                            cmd.AutoFit = true;
                        }
                    }

                    workSheet.Cells.AutoFitColumns();

                    workSheet.Cells[1, 1].Value = "Appointment Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 6])
                    {
                        Rng.Merge = true;
                        Rng.Style.Font.Bold = true;
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);

                    using (ExcelRange Rng = workSheet.Cells[2, 6])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true;
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    try
                    {
                        Byte[] theData = excel.GetAsByteArray();
                        string filename = "Appointments.xlsx";
                        string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(theData, mimeType, filename);
                    }
                    catch (Exception)
                    {
                        return BadRequest("Could not build and download the file.");
                    }
                }
            }
            return NotFound("No data.");
        }

        // Displays a paginated summary of appointments
        public async Task<IActionResult> AppointmentSummary(int? page, int? pageSizeID)
        {
            var sumQ = _context.AppointmentSummaries
                        .OrderBy(a => a.LastName)
                        .ThenBy(a => a.FirstName)
                        .AsNoTracking();

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "AppointmentSummary");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<AppointmentSummaryVM>.CreateAsync(sumQ.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // Displays a paginated summary of appointment reasons
        public async Task<IActionResult> AppointmentReasonSummary(int? page, int? pageSizeID)
        {
            var sumQ = _context.AppointmentReasonSummaries
                .OrderBy(a => a.ReasonName)
                .AsNoTracking();

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "AppointmentReasonSummary");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<AppointmentReasonSummaryVM>.CreateAsync(sumQ.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }
    }
}
