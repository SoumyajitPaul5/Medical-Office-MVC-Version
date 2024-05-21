using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    public class AppointmentSummaryVM
    {
        public int ID { get; set; }

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

        [Display(Name = "Number of Appointments")]
        public int NumberOfAppointments { get; set; }

        [Display(Name = "Total Extra Fees")]
        [DataType(DataType.Currency)]
        public double TotalExtraFees { get; set; }

        [Display(Name = "Maximum Fee Charged")]
        [DataType(DataType.Currency)]
        public double MaximumFeeCharged { get; set; }
    }
}
