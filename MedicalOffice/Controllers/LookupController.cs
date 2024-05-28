using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")] // Restricts access to Admin and Supervisor roles
    public class LookupController : CognizantController
    {
        private readonly MedicalOfficeContext _context;

        // Constructor to initialize the database context
        public LookupController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // Displays the main lookup page with the specified tab
        public IActionResult Index(string Tab = "Specialty-Tab")
        {
            ViewData["Tab"] = Tab;
            return View();
        }

        // Returns the partial view for conditions with a dropdown list of conditions
        public PartialViewResult Condition()
        {
            ViewData["ConditionID"] = new SelectList(_context.Conditions.OrderBy(a => a.ConditionName), "ID", "ConditionName");
            return PartialView("_Condition");
        }

        // Returns the partial view for specialties with a dropdown list of specialties
        public PartialViewResult Specialty()
        {
            ViewData["SpecialtyID"] = new SelectList(_context.Specialties.OrderBy(a => a.SpecialtyName), "ID", "SpecialtyName");
            return PartialView("_Specialty");
        }

        // Returns the partial view for appointment reasons with a dropdown list of reasons
        public PartialViewResult AppointmentReason()
        {
            ViewData["AppointmentReasonID"] = new SelectList(_context.AppointmentReasons.OrderBy(a => a.ReasonName), "ID", "ReasonName");
            return PartialView("_AppointmentReason");
        }

        // Returns the partial view for medical trials with a dropdown list of trials
        public PartialViewResult MedicalTrial()
        {
            ViewData["MedicalTrialID"] = new SelectList(_context.MedicalTrials.OrderBy(a => a.TrialName), "ID", "TrialName");
            return PartialView("_MedicalTrial");
        }
    }
}
