using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class AppointmentController : ElephantController
    {
        private readonly MedicalOfficeContext _context;

        public AppointmentController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(DateTime StartDate, DateTime EndDate, int? page, int? pageSizeID)
        {
            if (EndDate == DateTime.MinValue)
            {
                // Set StartDate and EndDate to the range of appointment dates
                StartDate = _context.Appointments.Min(o => o.StartTime).Date;
                EndDate = _context.Appointments.Max(o => o.StartTime).Date;
                ViewData["StartDate"] = StartDate.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = EndDate.ToString("yyyy-MM-dd");
            }
            if (EndDate < StartDate)
            {
                // Swap dates if EndDate is before StartDate
                DateTime temp = EndDate;
                EndDate = StartDate;
                StartDate = temp;
            }
            // Filter appointments within the date range
            var appointments = _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.StartTime >= StartDate && a.StartTime <= EndDate.AddDays(1))
                .OrderByDescending(a => a.StartTime);

            // Set page size for pagination
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            // Create paginated list of appointments
            var pagedData = await PaginatedList<Appointment>.CreateAsync(appointments, page ?? 1, pageSize);
            return View(pagedData);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            // Get appointment details
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Populate dropdown lists for creating a new appointment
            PopulateDropDownLists();
            Appointment appointment = new Appointment();
            return View(appointment);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartTime,EndTime,Notes,ExtraFee,DoctorID,PatientID,AppointmentReasonID")] Appointment appointment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Add new appointment to the database
                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { appointment.ID });
                }
            }
            catch (DbUpdateException)
            {
                // Handle database update exceptions
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(appointment);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            // Get appointment to edit
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(appointment);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var appointmentToUpdate = await _context.Appointments.FirstOrDefaultAsync(a => a.ID == id);

            if (appointmentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Appointment>(appointmentToUpdate, "",
                a => a.StartTime, a => a.EndTime, a => a.Notes, a => a.ExtraFee, a => a.DoctorID,
                a => a.PatientID, a => a.AppointmentReasonID))
            {
                try
                {
                    // Save changes to the appointment
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { appointmentToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency exceptions
                    if (!AppointmentExists(appointmentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    // Handle database update exceptions
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
                return RedirectToAction("Index", "Lookup", new { Tab = ControllerName() + "-Tab" });
            }

            PopulateDropDownLists(appointmentToUpdate);
            return View(appointmentToUpdate);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            // Get appointment to delete
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Appointments == null)
            {
                return Problem("No Appointments to Delete.");
            }
            var appointment = await _context.Appointments.FindAsync(id);
            try
            {
                // Delete the appointment
                if (appointment != null)
                {
                    _context.Appointments.Remove(appointment);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                // Handle database update exceptions
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(appointment);
        }

        // Helper method to populate dropdown lists for appointments
        private SelectList DoctorSelectList(int? selectedId)
        {
            return new SelectList(_context.Doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
        }

        private SelectList PatientSelectList(int? selectedId)
        {
            return new SelectList(_context.Patients.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
        }

        private SelectList AppointmentReasonSelectList(int? selectedId)
        {
            return new SelectList(_context.AppointmentReasons.OrderBy(m => m.ReasonName), "ID", "ReasonName", selectedId);
        }

        private void PopulateDropDownLists(Appointment appointment = null)
        {
            ViewData["DoctorID"] = DoctorSelectList(appointment?.DoctorID);
            ViewData["PatientID"] = PatientSelectList(appointment?.PatientID);
            ViewData["AppointmentReasonID"] = AppointmentReasonSelectList(appointment?.AppointmentReasonID);
        }

        // Helper method to check if an appointment exists
        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.ID == id);
        }
    }
}
