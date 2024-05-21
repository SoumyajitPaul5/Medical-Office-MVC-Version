using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class Doctor : Auditable
    {
        public int ID { get; set; }

        public string Summary => FullName;

        [Display(Name = "Doctor")]
        public string FullName
        {
            get
            {
                return "Dr. " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        [Display(Name = "City")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select the City where the Doctor first qualified.")]
        public int? CityID { get; set; }
        public City City { get; set; }

        public ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        [Display(Name = "Specialties")]
        public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new HashSet<DoctorSpecialty>();

        [Display(Name = "Documents")]
        public ICollection<DoctorDocument> DoctorDocuments { get; set; } = new HashSet<DoctorDocument>();
    }
}
