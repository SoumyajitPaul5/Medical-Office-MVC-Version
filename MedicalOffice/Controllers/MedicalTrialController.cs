using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.CustomControllers;
using Microsoft.AspNetCore.Authorization;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")] // Restricts access to Admin and Supervisor roles
    public class MedicalTrialController : LookupsController
    {
        private readonly IMyEmailSender _emailSender;
        private readonly MedicalOfficeContext _context;

        // Constructor to initialize the database context and email sender
        public MedicalTrialController(MedicalOfficeContext context, IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: MedicalTrial
        // Redirects to the return URL
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: MedicalTrial/Create
        // Displays the create view
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicalTrial/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TrialName")] MedicalTrial medicalTrial)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(medicalTrial);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                // Handles database update exceptions
                ExceptionMessageVM msg = new();
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    msg.ErrProperty = "TrialName";
                    msg.ErrMessage = "Unable to save changes. Remember, you cannot have duplicate Medical Trial Names.";
                }
                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }

            // Handles AJAX requests with error messages
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                return BadRequest(errorMessage);
            }

            return View(medicalTrial);
        }

        // GET: MedicalTrial/Edit/5
        // Retrieves and displays the edit view for a specific medical trial
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MedicalTrials == null)
            {
                return NotFound();
            }

            var medicalTrial = await _context.MedicalTrials.FindAsync(id);
            if (medicalTrial == null)
            {
                return NotFound();
            }
            return View(medicalTrial);
        }

        // POST: MedicalTrial/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var medicalTrialToUpdate = await _context.MedicalTrials.FirstOrDefaultAsync(m => m.ID == id);

            if (medicalTrialToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<MedicalTrial>(medicalTrialToUpdate, "", d => d.TrialName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalTrialExists(medicalTrialToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    // Handles database update exceptions
                    ExceptionMessageVM msg = new();
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        msg.ErrProperty = "TrialName";
                        msg.ErrMessage = "Unable to save changes. Remember, you cannot have duplicate Medical Trial Names.";
                    }
                    ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
                }
            }
            return View(medicalTrialToUpdate);
        }

        // GET: MedicalTrial/Delete/5
        // Retrieves and displays the delete view for a specific medical trial
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MedicalTrials == null)
            {
                return NotFound();
            }

            var medicalTrial = await _context.MedicalTrials.FirstOrDefaultAsync(m => m.ID == id);
            if (medicalTrial == null)
            {
                return NotFound();
            }

            return View(medicalTrial);
        }

        // POST: MedicalTrial/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MedicalTrials == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.MedicalTrials' is null.");
            }
            var medicalTrial = await _context.MedicalTrials.FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (medicalTrial != null)
                {
                    _context.MedicalTrials.Remove(medicalTrial);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                // Handles foreign key constraint exceptions
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] + " that has related records.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(medicalTrial);
        }

        // GET/POST: MedicalTrial/Notification/5
        // Handles sending notification emails to patients in a specific medical trial
        public async Task<IActionResult> Notification(int? id, string Subject, string emailContent)
        {
            if (id == null)
            {
                return NotFound();
            }
            MedicalTrial t = await _context.MedicalTrials.FindAsync(id);

            ViewData["id"] = id;
            ViewData["TrialName"] = t.TrialName;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    // Retrieves the list of patients in the medical trial
                    List<EmailAddress> folks = (from p in _context.Patients
                                                where p.MedicalTrialID == id
                                                select new EmailAddress
                                                {
                                                    Name = p.FullName,
                                                    Address = p.EMail
                                                }).ToList();
                    folksCount = folks.Count;
                    if (folksCount > 0)
                    {
                        // Sends the email notification
                        var msg = new EmailMessage()
                        {
                            ToAddresses = folks,
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"
                        };
                        await _emailSender.SendToManyAsync(msg);
                        ViewData["Message"] = "Message sent to " + folksCount + " Patient" + ((folksCount == 1) ? "." : "s.");
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent! No Patients in medical trial.";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Patient" + ((folksCount == 1) ? "" : "s") + " in the trial.";
                }
            }
            return View();
        }

        // Checks if a medical trial exists in the database
        private bool MedicalTrialExists(int id)
        {
            return _context.MedicalTrials.Any(e => e.ID == id);
        }
    }
}
