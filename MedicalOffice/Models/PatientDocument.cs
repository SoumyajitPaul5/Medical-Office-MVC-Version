using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a document associated with a patient
    public class PatientDocument : UploadedFile
    {
        // Foreign key for Patient
        [Display(Name = "Patient")]
        public int PatientID { get; set; }

        // Navigation property for Patient
        public Patient Patient { get; set; }
    }
}
