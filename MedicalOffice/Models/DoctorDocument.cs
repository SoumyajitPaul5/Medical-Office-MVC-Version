using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class DoctorDocument : UploadedFile
    {
        // Foreign key to the Doctor entity.
        [Display(Name = "Doctor")]
        public int DoctorID { get; set; }

        // Navigation property to the associated Doctor entity.
        public Doctor Doctor { get; set; }
    }
}
