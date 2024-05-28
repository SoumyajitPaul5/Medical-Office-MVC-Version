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
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class ConditionController : LookupsController
    {
        private readonly MedicalOfficeContext _context;

        public ConditionController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Condition
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Condition/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Condition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ConditionName")] Condition condition)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Add new condition to the database
                    _context.Add(condition);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                // Handle database update exceptions
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(condition);
        }

        // GET: Condition/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Conditions == null)
            {
                return NotFound();
            }

            // Get the condition to edit
            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }
            return View(condition);
        }

        // POST: Condition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var conditionToUpdate = await _context.Conditions.FirstOrDefaultAsync(m => m.ID == id);

            if (conditionToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Condition>(conditionToUpdate, "", d => d.ConditionName))
            {
                try
                {
                    // Save changes to the condition
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency exceptions
                    if (!ConditionExists(conditionToUpdate.ID))
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
            }
            return View(conditionToUpdate);
        }

        // GET: Condition/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Conditions == null)
            {
                return NotFound();
            }

            // Get the condition to delete
            var condition = await _context.Conditions.FirstOrDefaultAsync(m => m.ID == id);
            if (condition == null)
            {
                return NotFound();
            }

            return View(condition);
        }

        // POST: Condition/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Conditions == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.Conditions' is null.");
            }
            var condition = await _context.Conditions.FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                // Delete the condition
                if (condition != null)
                {
                    _context.Conditions.Remove(condition);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                // Handle database update exceptions
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
            return View(condition);
        }

        // Helper method to check if a condition exists
        private bool ConditionExists(int id)
        {
            return _context.Conditions.Any(e => e.ID == id);
        }
    }
}
