using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    // ViewModel for summarizing appointment reasons
    public class AppointmentReasonSummaryVM
    {
        public int ID { get; set; }

        // Reason for the appointment
        [Display(Name = "Reason for Apt.")]
        public string ReasonName { get; set; }

        // Number of appointments with this reason
        [Display(Name = "Number of Appointments")]
        public int NumberOfAppointments { get; set; }

        // Average age of patients for this appointment reason
        [Display(Name = "Average Patient Age")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double AverageAge { get; set; }

        // Total extra fees for this appointment reason
        [Display(Name = "Total Extra Fees")]
        [DataType(DataType.Currency)]
        public double TotalExtraFees { get; set; }

        // Maximum fee charged for this appointment reason
        [Display(Name = "Maximum Fee Charged")]
        [DataType(DataType.Currency)]
        public double MaximumFeeCharged { get; set; }
    }
}
