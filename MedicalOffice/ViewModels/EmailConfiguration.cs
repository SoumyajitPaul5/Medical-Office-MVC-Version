namespace MedicalOffice.ViewModels
{
    public class EmailConfiguration : IEmailConfiguration
    {
        // SMTP server address
        public string SmtpServer { get; set; }

        // SMTP server port
        public int SmtpPort { get; set; }

        // Name to use for the "From" field
        public string SmtpFromName { get; set; }

        // Username for SMTP authentication
        public string SmtpUsername { get; set; }

        // Password for SMTP authentication
        public string SmtpPassword { get; set; }
    }
}
