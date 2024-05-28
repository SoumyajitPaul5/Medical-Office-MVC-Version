namespace MedicalOffice.ViewModels
{
    public class EmailMessage
    {
        // List of recipient email addresses
        public List<EmailAddress> ToAddresses { get; set; } = new List<EmailAddress>();

        // Subject of the email
        public string Subject { get; set; }

        // Content of the email
        public string Content { get; set; }
    }
}
