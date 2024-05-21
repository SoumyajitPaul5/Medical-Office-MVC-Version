using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using MedicalOffice.Utilities;
using MedicalOffice.CustomControllers;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize]
    public class PatientController : ElephantController
    {
        private readonly MedicalOfficeContext _context;

        public PatientController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string SearchString, int? DoctorID, int? MedicalTrialID,
            int? page, int? pageSizeID, string actionButton, int? ConditionID,
            string sortDirection = "asc", string sortField = "Patient")
        {
            
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;


            string[] sortOptions = new[] { "Patient", "Age", "Visits/Yr", "Doctor" };

            ViewData["ConditionID"] = new SelectList(_context
                .Conditions
                .OrderBy(c => c.ConditionName), "ID", "ConditionName");

            PopulateDropDownLists();


            var patients = _context
                .Patients
                .Include(p => p.PatientThumbnail)
                .Include(p => p.Doctor)
                .Include(p => p.MedicalTrial)
                .Include(p=>p.PatientConditions).ThenInclude(p=>p.Condition)
                .AsNoTracking();

            if (DoctorID.HasValue)
            {
                patients = patients.Where(p => p.DoctorID == DoctorID);
                numberFilters++;
            }
            if (MedicalTrialID.HasValue)
            {
                patients = patients.Where(p => p.MedicalTrialID == MedicalTrialID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                patients = patients.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (ConditionID.HasValue)
            {
                patients = patients.Where(p => p.PatientConditions.Any(c => c.ConditionID == ConditionID));
                numberFilters++;
            }

            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";

            }

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
            if (sortField == "Visits/Yr")
            {
                if (sortDirection == "asc")
                {
                    patients = patients
                        .OrderBy(p => p.ExpYrVisits);
                }
                else
                {
                    patients = patients
                        .OrderByDescending(p => p.ExpYrVisits);
                }
            }
            else if (sortField == "Age")
            {
                if (sortDirection == "asc")
                {
                    patients = patients
                        .OrderByDescending(p => p.DOB);
                }
                else
                {
                    patients = patients
                        .OrderBy(p => p.DOB);
                }
            }
            else if (sortField == "Doctor")
            {
                if (sortDirection == "asc")
                {
                    patients = patients
                        .OrderBy(p => p.Doctor.LastName)
                        .ThenBy(p => p.Doctor.FirstName);
                }
                else
                {
                    patients = patients
                        .OrderByDescending(p => p.Doctor.LastName)
                        .ThenByDescending(p => p.Doctor.FirstName);
                }
            }
            else 
            {
                if (sortDirection == "asc")
                {
                    patients = patients
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    patients = patients
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Patient>.CreateAsync(patients.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Patients/Details/5
        [Authorize(Roles ="Admin,Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.PatientPhoto)
                .Include(p => p.Doctor)
                .Include(p => p.MedicalTrial)
                .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Admin,Supervisor")]
        public IActionResult Create()
        {

            var patient = new Patient();
            PopulateAssignedConditionData(patient);
            PopulateDropDownLists();
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Create([Bind("ID,OHIP,FirstName,MiddleName,LastName,DOB," +
            "ExpYrVisits,Phone,EMail,MedicalTrialID,DoctorID")] Patient patient, 
            string[] selectedOptions, IFormFile thePicture)
        {
            try
            {
                if (selectedOptions != null)
                {
                    foreach (var condition in selectedOptions)
                    {
                        var conditionToAdd = new PatientCondition { PatientID = patient.ID, ConditionID = int.Parse(condition) };
                        patient.PatientConditions.Add(conditionToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    await AddPicture(patient, thePicture);
                    _context.Add(patient);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "PatientAppointment", new { PatientID = patient.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                {
                    ModelState.AddModelError("OHIP", "Unable to save changes. Remember, you cannot have duplicate OHIP numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateAssignedConditionData(patient);
            PopulateDropDownLists(patient);
            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.PatientPhoto)
                .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
                .FirstOrDefaultAsync(p => p.ID == id);
            if (patient == null)
            {
                return NotFound();
            }
            PopulateAssignedConditionData(patient);
            PopulateDropDownLists(patient);
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions, 
            Byte[] RowVersion, string chkRemoveImage, IFormFile thePicture)
        {
            var patientToUpdate = await _context.Patients
                .Include(p => p.PatientPhoto)
                .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (patientToUpdate == null)
            {
                return NotFound();
            }

            UpdatePatientConditions(selectedOptions, patientToUpdate);

            _context.Entry(patientToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Patient>(patientToUpdate, "",
                p=>p.OHIP, p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.DOB,
                p => p.ExpYrVisits, p => p.Phone, p => p.EMail, p => p.MedicalTrialID, p => p.DoctorID))
            {
                try
                {
                    if (chkRemoveImage != null)
                    {
                        patientToUpdate.PatientThumbnail = _context.PatientThumbnails.Where(p => p.PatientID == patientToUpdate.ID).FirstOrDefault();
                        patientToUpdate.PatientPhoto = null;
                        patientToUpdate.PatientThumbnail = null;
                    }
                    else
                    {
                        await AddPicture(patientToUpdate, thePicture);
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "PatientAppointment", new { PatientID = patientToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Patient)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Patient was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Patient)databaseEntry.ToObject();
                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", "Current value: "
                                + databaseValues.FirstName);
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", "Current value: "
                                + databaseValues.MiddleName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", "Current value: "
                                + databaseValues.LastName);
                        if (databaseValues.OHIP != clientValues.OHIP)
                            ModelState.AddModelError("OHIP", "Current value: "
                                + databaseValues.OHIP);
                        if (databaseValues.DOB != clientValues.DOB)
                            ModelState.AddModelError("DOB", "Current value: "
                                + String.Format("{0:d}", databaseValues.DOB));
                        if (databaseValues.Phone != clientValues.Phone)
                            ModelState.AddModelError("Phone", "Current value: "
                                + databaseValues.PhoneFormatted);
                        if (databaseValues.EMail != clientValues.EMail)
                            ModelState.AddModelError("EMail", "Current value: "
                                + databaseValues.EMail);
                        if (databaseValues.ExpYrVisits != clientValues.ExpYrVisits)
                            ModelState.AddModelError("ExpYrVisits", "Current value: "
                                + databaseValues.ExpYrVisits);
                        if (databaseValues.DoctorID != clientValues.DoctorID)
                        {
                            Doctor databaseDoctor = await _context.Doctors.FirstOrDefaultAsync(i => i.ID == databaseValues.DoctorID);
                            ModelState.AddModelError("DoctorID", $"Current value: {databaseDoctor?.FullName}");
                        }

                        if (databaseValues.MedicalTrialID != clientValues.MedicalTrialID)
                        {
                            if (databaseValues.MedicalTrialID.HasValue)
                            {
                                MedicalTrial databaseMedicalTrial = await _context.MedicalTrials.FirstOrDefaultAsync(i => i.ID == databaseValues.MedicalTrialID);
                                ModelState.AddModelError("MedicalTrialID", $"Current value: {databaseMedicalTrial?.TrialName}");
                            }
                            else

                            {
                                ModelState.AddModelError("MedicalTrialID", $"Current value: None");
                            }
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to Patient List' hyperlink.");
                        patientToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }

                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE"))
                    {
                        ModelState.AddModelError("OHIP", "Unable to save changes. Remember, you cannot have duplicate OHIP numbers.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            PopulateAssignedConditionData(patientToUpdate);
            PopulateDropDownLists(patientToUpdate);
            return View(patientToUpdate);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.MedicalTrial)
                .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Patients == null)
            {
                return Problem("Patient has already been deleted from the system.");
            }
            var patient = await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.MedicalTrial)
                .Include(p => p.PatientConditions).ThenInclude(p => p.Condition)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (patient != null)
                {
                    _context.Patients.Remove(patient);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(patient);
        }

 
        private SelectList DoctorSelectList(int? selectedId)
        {
            return new SelectList(_context.Doctors
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
        }
        private SelectList MedicalTrialList(int? selectedId)
        {
            return new SelectList(_context
                .MedicalTrials
                .OrderBy(m => m.TrialName), "ID", "TrialName", selectedId);
        }
        private void PopulateDropDownLists(Patient patient = null)
        {
            ViewData["DoctorID"] = DoctorSelectList(patient?.DoctorID);
            ViewData["MedicalTrialID"] = MedicalTrialList(patient?.MedicalTrialID);
        }

        [HttpGet]
        public JsonResult GetMedicalTrials(int? id)
        {
            return Json(MedicalTrialList(id));
        }

        private void PopulateAssignedConditionData(Patient patient)
        {

            var allOptions = _context.Conditions;
            var currentOptionIDs = new HashSet<int>(patient.PatientConditions.Select(b => b.ConditionID));
            var checkBoxes = new List<CheckOptionVM>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new CheckOptionVM
                {
                    ID = option.ID,
                    DisplayText = option.ConditionName,
                    Assigned = currentOptionIDs.Contains(option.ID)
                });
            }
            ViewData["ConditionOptions"] = checkBoxes;
        }
        private void UpdatePatientConditions(string[] selectedOptions, Patient patientToUpdate)
        {
            if (selectedOptions == null)
            {
                patientToUpdate.PatientConditions = new List<PatientCondition>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var patientOptionsHS = new HashSet<int>
                (patientToUpdate.PatientConditions.Select(c => c.ConditionID));
            foreach (var option in _context.Conditions)
            {
                if (selectedOptionsHS.Contains(option.ID.ToString()))
                {
                    if (!patientOptionsHS.Contains(option.ID)) 
                    {
                        patientToUpdate.PatientConditions.Add(new PatientCondition { PatientID = patientToUpdate.ID, ConditionID = option.ID });
                    }
                }
                else
                {
                    if (patientOptionsHS.Contains(option.ID)) 
                    {
                        PatientCondition conditionToRemove = patientToUpdate.PatientConditions.SingleOrDefault(c => c.ConditionID == option.ID);
                        _context.Remove(conditionToRemove);
                    }
                }
            }
        }

        private async Task AddPicture(Patient patient, IFormFile thePicture)
        {
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))
                {
                    if (mimeType.Contains("image"))
                    {
                        using var memoryStream = new MemoryStream();
                        await thePicture.CopyToAsync(memoryStream);
                        var pictureArray = memoryStream.ToArray();

                        if (patient.PatientPhoto != null)
                        {
                            patient.PatientPhoto.Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600);

                            patient.PatientThumbnail = _context.PatientThumbnails.Where(p => p.PatientID == patient.ID).FirstOrDefault();
                            patient.PatientThumbnail.Content = ResizeImage.shrinkImageWebp(pictureArray, 75, 90);
                        }
                        else 
                        {
                            patient.PatientPhoto = new PatientPhoto
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 500, 600),
                                MimeType = "image/webp"
                            };
                            patient.PatientThumbnail = new PatientThumbnail
                            {
                                Content = ResizeImage.shrinkImageWebp(pictureArray, 75, 90),
                                MimeType = "image/webp"
                            };
                        }
                    }
                }
            }
        }

        private bool PatientExists(int id)
        {
          return _context.Patients.Any(e => e.ID == id);
        }
    }
}
