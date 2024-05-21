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
                StartDate = _context.Appointments.Min(o => o.StartTime).Date;
                EndDate = _context.Appointments.Max(o => o.StartTime).Date;
                ViewData["StartDate"] = StartDate.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = EndDate.ToString("yyyy-MM-dd");
            }
            if (EndDate < StartDate)
            {
                DateTime temp = EndDate;
                EndDate = StartDate;
                StartDate = temp;
            }
            var appointments = _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.StartTime >= StartDate && a.StartTime <= EndDate.AddDays(1))
                .OrderByDescending(a=>a.StartTime);

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

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
            PopulateDropDownLists();
            Appointment appointment = new Appointment();
            return View(appointment);
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartTime,EndTime,Notes,ExtraFee,DoctorID,PatientID,AppointmentReasonID")] Appointment appointment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { appointment.ID });
                }
            }
            catch (DbUpdateException)
            {
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

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(appointment);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the patient to update
            var appointmentToUpdate = await _context.Appointments
                .FirstOrDefaultAsync(a => a.ID == id);

            //Check that you got it or exit with a not found error
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
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { appointmentToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
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
                return Problem("No Appopintments to Delete.");
            }
            var appointment = await _context.Appointments.FindAsync(id);
            try
            {
                if (appointment != null)
                {
                    _context.Appointments.Remove(appointment);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(appointment);

        }

        private SelectList DoctorSelectList(int? selectedId)
        {
            return new SelectList(_context.Doctors
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
        }
        private SelectList PatientSelectList(int? selectedId)
        {
            return new SelectList(_context.Patients
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
        }
        private SelectList AppointmentReasonSelectList(int? selectedId)
        {
            return new SelectList(_context
                .AppointmentReasons
                .OrderBy(m => m.ReasonName), "ID", "ReasonName", selectedId);
        }
        private void PopulateDropDownLists(Appointment appointment = null)
        {
            ViewData["DoctorID"] = DoctorSelectList(appointment?.DoctorID);
            ViewData["PatientID"] = PatientSelectList(appointment?.PatientID);
            ViewData["AppointmentReasonID"] = AppointmentReasonSelectList(appointment?.AppointmentReasonID);
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.ID == id);
        }
    }
}
