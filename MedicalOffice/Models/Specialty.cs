using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a medical specialty in the system
    public class Specialty
    {
        public int ID { get; set; }

        [Display(Name = "Medical Specialty")]
        [Required(ErrorMessage = "You cannot leave the name of the Specialty blank.")]
        [StringLength(100, ErrorMessage = "Too Big!")]
        public string SpecialtyName { get; set; }

        // Collection of doctor-specialty relationships
        public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new HashSet<DoctorSpecialty>();
    }
}
