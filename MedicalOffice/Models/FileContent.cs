using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class FileContent
    {
        // Primary key for FileContent, also a foreign key to UploadedFile
        [Key, ForeignKey("UploadedFile")]
        public int FileContentID { get; set; }

        // The content of the file, not to be scaffolded in views
        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        // Navigation property to the associated UploadedFile
        public UploadedFile UploadedFile { get; set; }
    }
}
