using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a medical trial
    public class MedicalTrial
    {
        // Unique identifier for the trial
        public int ID { get; set; }

        // Name of the trial
        [Display(Name = "Trial Name")]
        [Required(ErrorMessage = "You cannot leave the name of the trial blank.")]
        [StringLength(250, ErrorMessage = "Trial name cannot be more than 250 characters long.")]
        [DataType(DataType.MultilineText)]
        [DisplayFormat(NullDisplayText = "None")]
        public string TrialName { get; set; }

        // Collection of patients associated with the trial
        public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();
    }
}
