using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    // ViewModel for summarizing patient appointments
    public class AppointmentSummaryVM
    {
        public int ID { get; set; }

        // Computed property to return the patient's full name
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

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        // Number of appointments the patient has
        [Display(Name = "Number of Appointments")]
        public int NumberOfAppointments { get; set; }

        // Total extra fees for the patient's appointments
        [Display(Name = "Total Extra Fees")]
        [DataType(DataType.Currency)]
        public double TotalExtraFees { get; set; }

        // Maximum fee charged for any of the patient's appointments
        [Display(Name = "Maximum Fee Charged")]
        [DataType(DataType.Currency)]
        public double MaximumFeeCharged { get; set; }
    }
}
