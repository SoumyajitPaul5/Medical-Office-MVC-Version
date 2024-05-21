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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ConditionName")] Condition condition)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(condition);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
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

            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }
            return View(condition);
        }

        // POST: Condition/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var conditionToUpdate = await _context.Conditions
                .FirstOrDefaultAsync(m => m.ID == id);

            if (conditionToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Condition>(conditionToUpdate, "",
                d => d.ConditionName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
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

            var condition = await _context.Conditions
                .FirstOrDefaultAsync(m => m.ID == id);
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
                return Problem("Entity set 'MedicalOfficeContext.Conditions'  is null.");
            }
            var condition = await _context.Conditions
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (condition != null)
                {
                    _context.Conditions.Remove(condition);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
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

        private bool ConditionExists(int id)
        {
          return _context.Conditions.Any(e => e.ID == id);
        }
    }
}
