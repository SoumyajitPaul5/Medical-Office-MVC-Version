using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a photo of a patient
    public class PatientPhoto
    {
        // Unique identifier for the photo
        public int ID { get; set; }

        // Binary content of the photo
        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        // MIME type of the photo
        [StringLength(255)]
        public string MimeType { get; set; }

        // Foreign key to the associated patient
        public int PatientID { get; set; }

        // Navigation property to the associated patient
        public Patient Patient { get; set; }
    }
}
