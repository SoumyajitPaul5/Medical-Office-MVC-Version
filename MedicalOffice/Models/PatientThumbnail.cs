using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a thumbnail image of a patient
    public class PatientThumbnail
    {
        // Unique identifier for the thumbnail
        public int ID { get; set; }

        // Binary content of the thumbnail image
        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        // MIME type of the thumbnail image
        [StringLength(255)]
        public string MimeType { get; set; }

        // Foreign key to the associated patient
        public int PatientID { get; set; }

        // Navigation property to the associated patient
        public Patient Patient { get; set; }
    }
}
