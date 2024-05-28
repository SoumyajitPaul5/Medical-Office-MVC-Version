namespace MedicalOffice.ViewModels
{
    public class ExceptionMessageVM
    {
        // Property where the error occurred
        public string ErrProperty { get; set; } = string.Empty;

        // Error message to display
        public string ErrMessage { get; set; } = "Unable to save changes. " +
            "Try again, and if the problem persists see your system administrator.";
    }
}
