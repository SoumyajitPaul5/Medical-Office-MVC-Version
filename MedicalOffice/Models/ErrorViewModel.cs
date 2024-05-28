namespace MedicalOffice.Models
{
    public class ErrorViewModel
    {
        // Holds the unique identifier for the request.
        public string RequestId { get; set; }

        // Indicates whether the RequestId should be shown.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
