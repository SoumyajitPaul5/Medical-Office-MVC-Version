using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    public class AppointmentReasonSummaryVM
    {
        public int ID { get; set; }

        [Display(Name = "Reason for Apt.")]
        public string ReasonName { get; set; }

        [Display(Name = "Number of Appointments")]
        public int NumberOfAppointments { get; set; }

        [Display(Name = "Average Patient Age")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double AverageAge { get; set; }

        [Display(Name = "Total Extra Fees")]
        [DataType(DataType.Currency)]
        public double TotalExtraFees { get; set; }

        [Display(Name = "Maximum Fee Charged")]
        [DataType(DataType.Currency)]
        public double MaximumFeeCharged { get; set; }
    }
}
