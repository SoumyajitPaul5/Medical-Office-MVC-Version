using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class MedicalTrial 
    {
        public int ID { get; set; }

        [Display(Name = "Trial Name")]
        [Required(ErrorMessage = "You cannot leave the name of the trial blank.")]
        [StringLength(250, ErrorMessage = "Trial name cannot be more than 250 characters long.")]
        [DataType(DataType.MultilineText)]
        [DisplayFormat(NullDisplayText = "None")]
        public string TrialName { get; set; }

        public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();

    }
}
