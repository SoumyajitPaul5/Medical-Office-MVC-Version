using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.Utilities;
using MedicalOffice.CustomControllers;
using MedicalOffice.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize]
    public class DoctorController : ElephantController
    {
        private readonly MedicalOfficeContext _context;

        public DoctorController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index(string SearchString, string SearchDoctor, int? SpecialtyID, int? page, int? pageSizeID)
        {
            ViewData["SpecialtyID"] = new SelectList(_context
                .Specialties
                .OrderBy(s => s.SpecialtyName), "ID", "SpecialtyName");

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            var doctors = _context.Doctors
                 .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                 .Include(d => d.DoctorDocuments)
                 .Include(d => d.City)
                 .AsNoTracking();

            if (SpecialtyID.HasValue)
            {
                doctors = doctors.Where(p => p.DoctorSpecialties.Any(c => c.SpecialtyID == SpecialtyID));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                doctors = doctors.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchDoctor))
            {
                doctors = doctors.Where(p => p.FirstName.ToUpper()
                + " " + p.LastName.ToUpper() == SearchDoctor.ToUpper());
            }
            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
            }
            doctors = doctors
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Doctor>.CreateAsync(doctors, page ?? 1, pageSize);

            if (User.IsInRole("Admin"))
            {
                return View("IndexAdmin", pagedData);
            }
            else if (User.IsInRole("Supervisor"))
            {
                return View("IndexSupervisor", pagedData);
            }
            else
            {
                return View(pagedData);
            }
        }

        // GET: Doctors/Details/5
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context
                .Doctors
                .Include(d => d.City)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            Doctor doctor = new Doctor();
            PopulateAssignedSpecialtyData(doctor);
            PopulateDropDownLists();
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName," +
            "LastName, CityID")] Doctor doctor, string[] selectedOptions, List<IFormFile> theFiles)
        {
            try
            {
                UpdateDoctorSpecialties(selectedOptions, doctor);
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(doctor, theFiles);
                    _context.Add(doctor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { doctor.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            //Validation Error so give the user another chance.
            PopulateAssignedSpecialtyData(doctor);
            PopulateDropDownLists(doctor);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
               .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
               .Include(d => d.DoctorDocuments)
               .Include(d => d.City)
               .FirstOrDefaultAsync(d => d.ID == id);

            if (doctor == null)
            {
                return NotFound();
            }

            PopulateAssignedSpecialtyData(doctor);
            PopulateDropDownLists(doctor);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions, List<IFormFile> theFiles)
        {
            //Go get the Doctor to update
            var doctorToUpdate = await _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .Include(d => d.City)
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (doctorToUpdate == null)
            {
                return NotFound();
            }

            //Update the Doctor's Specialties
            UpdateDoctorSpecialties(selectedOptions, doctorToUpdate);

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Doctor>(doctorToUpdate, "",
                d => d.FirstName, d => d.MiddleName, d => d.LastName, d => d.CityID))
            {
                try
                {
                    await AddDocumentsAsync(doctorToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { doctorToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctorToUpdate.ID))
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

            //Validation Error so give the user another chance.
            PopulateAssignedSpecialtyData(doctorToUpdate);
            PopulateDropDownLists(doctorToUpdate);
            return View(doctorToUpdate);
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                 .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                 .Include(d => d.DoctorDocuments)
                 .Include(d => d.City)
                 .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Doctors == null)
            {
                return Problem("No Doctors to Delete.");
            }
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .Include(d => d.City)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (doctor != null)
                {
                    _context.Doctors.Remove(doctor);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Doctor. Remember, you cannot delete a Doctor that has patients assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(doctor);
        }

        public PartialViewResult ListOfSpecialtiesDetails(int id)
        {
            var query = from s in _context.DoctorSpecialties.Include(p => p.Specialty)
                        where s.DoctorID == id
                        orderby s.Specialty.SpecialtyName
                        select s;
            return PartialView("_ListOfSpecialities", query.ToList());
        }

        public PartialViewResult ListOfPatientsDetails(int id)
        {
            var query = from p in _context.Patients
                        where p.DoctorID == id
                        orderby p.LastName, p.FirstName
                        select p;
            return PartialView("_ListOfPatients", query.ToList());
        }

        public PartialViewResult ListOfDocumentsDetails(int id)
        {
            var query = from p in _context.DoctorDocuments
                        where p.DoctorID == id
                        orderby p.FileName
                        select p;
            return PartialView("_ListOfDocuments", query.ToList());
        }
        public JsonResult GetDoctors(string term)
        {
            var result = from d in _context.Doctors
                         where d.LastName.ToUpper().Contains(term.ToUpper())
                               || d.FirstName.ToUpper().Contains(term.ToUpper())
                         orderby d.FirstName, d.LastName
                         select new { value = d.FirstName + " " + d.LastName };
            return Json(result);
        }
        private SelectList ProvinceSelectList(string selectedId)
        {
            return new SelectList(_context.Provinces
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList CitySelectList(string ProvinceID, int? selectedId)
        {
            var query = from c in _context.Cities
                        where c.ProvinceID == ProvinceID
                        select c;
            return new SelectList(query.OrderBy(p => p.Name), "ID", "Summary", selectedId);
        }
        private void PopulateDropDownLists(Doctor doctor = null)
        {

            if ((doctor?.CityID).HasValue)
            {   
                if(doctor.City==null) {
                    doctor.City = _context.Cities.Find(doctor.CityID);
                }
                ViewData["ProvinceID"] = ProvinceSelectList(doctor.City.ProvinceID);
                ViewData["CityID"] = CitySelectList(doctor.City.ProvinceID, doctor.CityID);
            }
            else
            {
                ViewData["ProvinceID"] = ProvinceSelectList(null);
                ViewData["CityID"] = CitySelectList(null, null);
            }
        }

        [HttpGet]
        public JsonResult GetCities(string ProvinceID)
        {
            return Json(CitySelectList(ProvinceID, null));
        }

        private SelectList SpecialtySelectList(string skip)
        {
            var SpecialtyQuery = _context.Specialties
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                SpecialtyQuery = SpecialtyQuery
                    .Where(s => !skipKeys.Contains(s.ID));
            }
            return new SelectList(SpecialtyQuery.OrderBy(d => d.SpecialtyName.ToLower()), "ID", "SpecialtyName");
        }
        [HttpGet]
        public JsonResult GetSpecialties(string skip)
        {
            return Json(SpecialtySelectList(skip));
        }

        private void PopulateAssignedSpecialtyData(Doctor doctor)
        {
            var allOptions = _context.Specialties;
            var currentOptionsHS = new HashSet<int>(doctor.DoctorSpecialties.Select(b => b.SpecialtyID));
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.SpecialtyName
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.SpecialtyName
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateDoctorSpecialties(string[] selectedOptions, Doctor doctorToUpdate)
        {
            if (selectedOptions == null)
            {
                doctorToUpdate.DoctorSpecialties = new List<DoctorSpecialty>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(doctorToUpdate.DoctorSpecialties.Select(b => b.SpecialtyID));
            foreach (var s in _context.Specialties)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))
                {
                    if (!currentOptionsHS.Contains(s.ID))
                    {
                        doctorToUpdate.DoctorSpecialties.Add(new DoctorSpecialty
                        {
                            SpecialtyID = s.ID,
                            DoctorID = doctorToUpdate.ID
                        });
                    }
                }
                else 
                {
                    if (currentOptionsHS.Contains(s.ID))
                    {
                        DoctorSpecialty specToRemove = doctorToUpdate.DoctorSpecialties.FirstOrDefault(d => d.SpecialtyID == s.ID);
                        _context.Remove(specToRemove);
                    }
                }
            }
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Doctor doctor, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    
                    if (!(fileName == "" || fileLength == 0))
                    {
                        DoctorDocument d = new DoctorDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        doctor.DoctorDocuments.Add(d);
                    };
                }
            }
        }
        private bool DoctorExists(int id)
        {
          return _context.Doctors.Any(e => e.ID == id);
        }
    }
}
