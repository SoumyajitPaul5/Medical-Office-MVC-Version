using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class LookupController : CognizantController
    {
        private readonly MedicalOfficeContext _context;

        public LookupController(MedicalOfficeContext context)
        {
            _context = context;
        }
        public IActionResult Index(string Tab = "Specialty-Tab")
        {
            ViewData["Tab"] = Tab;
            return View();
        }
        public PartialViewResult Condition()
        {
            ViewData["ConditionID"] = new
                SelectList(_context.Conditions
                .OrderBy(a => a.ConditionName), "ID", "ConditionName");
            return PartialView("_Condition");
        }
        public PartialViewResult Specialty()
        {
            ViewData["SpecialtyID"] = new
                SelectList(_context.Specialties
                .OrderBy(a => a.SpecialtyName), "ID", "SpecialtyName");
            return PartialView("_Specialty");
        }
        public PartialViewResult AppointmentReason()
        {
            ViewData["AppointmentReasonID"] = new
                SelectList(_context.AppointmentReasons
                .OrderBy(a => a.ReasonName), "ID", "ReasonName");
            return PartialView("_AppointmentReason");
        }
        public PartialViewResult MedicalTrial()
        {
            ViewData["MedicalTrialID"] = new
                SelectList(_context.MedicalTrials
                .OrderBy(a => a.TrialName), "ID", "TrialName");
            return PartialView("_MedicalTrial");
        }

    }
}
