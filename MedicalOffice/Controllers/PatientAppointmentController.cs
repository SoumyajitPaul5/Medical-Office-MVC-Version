using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using Microsoft.AspNetCore.Authorization;
using MedicalOffice.CustomControllers;
using MedicalOffice.Utilities;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")] // Restricts access to Admin and Supervisor roles
    public class PatientAppointmentController : ElephantController
    {
        private readonly MedicalOfficeContext _context;

        // Constructor to initialize the database context
        public PatientAppointmentController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: PatientAppointment
        // Displays the list of appointments for a specific patient with sorting, filtering, and pagination
        public async Task<IActionResult> Index(int? PatientID, int? page, int? pageSizeID, int? AppointmentReasonID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "Appointment")
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Patient");

            if (!PatientID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            string[] sortOptions = new[] { "Appointment", "Appt. Reason", "Extra Fees" };

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            // Retrieves the list of appointments for the specified patient
            var appts = from a in _context.Appointments
                        .Include(a => a.AppointmentReason)
                        .Include(a => a.Patient)
                        .Include(a => a.Doctor)
                        where a.PatientID == PatientID.GetValueOrDefault()
                        select a;

            // Applies filters
            if (AppointmentReasonID.HasValue)
            {
                appts = appts.Where(p => p.AppointmentReasonID == AppointmentReasonID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                appts = appts.Where(p => p.Notes.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = "(" + numberFilters.ToString() + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
            }

            // Handles sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }

            if (sortField == "Appt. Reason")
            {
                appts = sortDirection == "asc" ? appts.OrderBy(p => p.AppointmentReason.ReasonName) : appts.OrderByDescending(p => p.AppointmentReason.ReasonName);
            }
            else if (sortField == "Extra Fees")
            {
                appts = sortDirection == "asc" ? appts.OrderBy(p => p.ExtraFee) : appts.OrderByDescending(p => p.ExtraFee);
            }
            else
            {
                appts = sortDirection == "asc" ? appts.OrderByDescending(p => p.StartTime) : appts.OrderBy(p => p.StartTime);
            }
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Retrieves the patient details
            Patient patient = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.MedicalTrial)
                .Include(p => p.PatientThumbnail)
                .Include(p => p.PatientConditions).ThenInclude(pc => pc.Condition)
                .Where(p => p.ID == PatientID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Patient = patient;

            // Handles pagination
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Appointment>.CreateAsync(appts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: PatientAppointment/Add
        // Displays the form to add a new appointment for a specific patient
        public IActionResult Add(int? PatientID, string PatientName)
        {
            if (!PatientID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }
            ViewData["PatientName"] = PatientName;

            Appointment a = new Appointment()
            {
                PatientID = PatientID.GetValueOrDefault()
            };

            PopulateDropDownLists();
            return View(a);
        }

        // POST: PatientAppointment/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("StartTime,EndTime,Notes,ExtraFee,DoctorID,PatientID,AppointmentReasonID")] Appointment appointment, string PatientName)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(appointment);
            ViewData["PatientName"] = PatientName;
            return View(appointment);
        }

        // GET: PatientAppointment/Update/5
        // Displays the form to update a specific appointment
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
               .Include(a => a.AppointmentReason)
               .Include(a => a.Patient)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);

            if (appointment == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(appointment);
            return View(appointment);
        }

        // POST: PatientAppointment/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var appointmentToUpdate = await _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (appointmentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Appointment>(appointmentToUpdate, "", a => a.StartTime, a => a.EndTime, a => a.Notes, a => a.ExtraFee, a => a.DoctorID, a => a.AppointmentReasonID))
            {
                try
                {
                    _context.Update(appointmentToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
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
            }
            PopulateDropDownLists(appointmentToUpdate);
            return View(appointmentToUpdate);
        }

        // GET: PatientAppointment/Remove/5
        // Displays the form to confirm removal of a specific appointment
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.AppointmentReason)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: PatientAppointment/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            if (_context.Appointments == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.Appointments' is null.");
            }

            var appointment = await _context.Appointments
               .Include(a => a.AppointmentReason)
               .Include(a => a.Patient)
               .Include(a => a.Doctor)
               .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(appointment);
        }

        // Retrieves a SelectList of appointment reasons
        private SelectList AppointmentReasonSelectList(int? id)
        {
            var dQuery = from d in _context.AppointmentReasons orderby d.ReasonName select d;
            return new SelectList(dQuery, "ID", "ReasonName", id);
        }

        // Retrieves a SelectList of doctors
        private SelectList DoctorSelectList(int? id)
        {
            var dQuery = from d in _context.Doctors orderby d.LastName, d.FirstName select d;
            return new SelectList(dQuery, "ID", "FormalName", id);
        }

        // Populates dropdown lists for appointment reasons and doctors
        private void PopulateDropDownLists(Appointment appointment = null)
        {
            ViewData["AppointmentReasonID"] = AppointmentReasonSelectList(appointment?.AppointmentReasonID);
            ViewData["DoctorID"] = DoctorSelectList(appointment?.DoctorID);
        }

        // Checks if an appointment exists in the database
        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.ID == id);
        }
    }
}
