using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    // Represents a patient in the medical office system
    public class Patient : Auditable, IValidatableObject
    {
        // Unique identifier for the patient
        public int ID { get; set; }

        // Full name of the patient
        [Display(Name = "Patient")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        // Formal name of the patient
        [Display(Name = "Patient")]
        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        // Age of the patient calculated from DOB
        public string Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int? a = today.Year - DOB?.Year
                    - ((today.Month < DOB?.Month || (today.Month == DOB?.Month && today.Day < DOB?.Day) ? 1 : 0));
                return a?.ToString();
            }
        }

        // Summary of the age and DOB of the patient
        [Display(Name = "Age (DOB)")]
        public string AgeSummary
        {
            get
            {
                string ageSummary = "Unknown";
                if (DOB.HasValue)
                {
                    ageSummary = Age + " (" + String.Format("{0:yyyy-MM-dd}", DOB) + ")";
                }
                return ageSummary;
            }
        }

        // Formatted phone number of the patient
        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone[6..];
            }
        }

        // OHIP number of the patient
        [Required(ErrorMessage = "You cannot leave the OHIP number blank.")]
        [StringLength(10)]
        [RegularExpression("^\\d{10}$", ErrorMessage = "The OHIP number must be exactly 10 numeric digits.")]
        public string OHIP { get; set; }

        // First name of the patient
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        // Middle name of the patient
        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string MiddleName { get; set; }

        // Last name of the patient
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        // Date of birth of the patient
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }

        // Expected visits per year
        [Display(Name = "Visits/Yr")]
        [Required(ErrorMessage = "You cannot leave the number of expected visits per year blank.")]
        [Range(1, 12, ErrorMessage = "The number of expected visits per year must be between 1 and 12.")]
        public byte ExpYrVisits { get; set; }

        // Phone number of the patient
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; }

        // Email address of the patient
        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string EMail { get; set; }

        // Row version for concurrency
        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }

        // Primary care physician ID
        [Required(ErrorMessage = "You must select a Primary Care Physician.")]
        [Display(Name = "Doctor")]
        public int DoctorID { get; set; }

        // Primary care physician
        [Display(Name = "Doctor")]
        public Doctor Doctor { get; set; }

        // Medical trial ID
        [Display(Name = "Medical Trial")]
        public int? MedicalTrialID { get; set; }

        // Medical trial
        [Display(Name = "Medical Trial")]
        public MedicalTrial MedicalTrial { get; set; }

        // Patient conditions history
        [Display(Name = "History")]
        public ICollection<PatientCondition> PatientConditions { get; set; } = new HashSet<PatientCondition>();

        // Appointments of the patient
        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        // Patient photo
        public PatientPhoto PatientPhoto { get; set; }

        // Patient thumbnail photo
        public PatientThumbnail PatientThumbnail { get; set; }

        // Documents of the patient
        [Display(Name = "Documents")]
        public ICollection<PatientDocument> PatientDocuments { get; set; } = new HashSet<PatientDocument>();

        // Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DOB.GetValueOrDefault() > DateTime.Today)
            {
                yield return new ValidationResult("Date of Birth cannot be in the future.", new[] { "DOB" });
            }
        }
    }
}
