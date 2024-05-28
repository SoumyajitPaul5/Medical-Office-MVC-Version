namespace MedicalOffice.Models
{
    public class DoctorSpecialty
    {
        // Foreign key to the Doctor entity.
        public int DoctorID { get; set; }

        // Navigation property to the associated Doctor entity.
        public Doctor Doctor { get; set; }

        // Foreign key to the Specialty entity.
        public int SpecialtyID { get; set; }

        // Navigation property to the associated Specialty entity.
        public Specialty Specialty { get; set; }
    }
}
