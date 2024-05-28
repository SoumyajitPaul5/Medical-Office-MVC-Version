namespace MedicalOffice.ViewModels
{
    public interface IEmailConfiguration
    {
        // SMTP server address
        string SmtpServer { get; }

        // SMTP server port
        int SmtpPort { get; }

        // Name displayed in the 'From' field of the email
        string SmtpFromName { get; set; }

        // SMTP server username
        string SmtpUsername { get; set; }

        // SMTP server password
        string SmtpPassword { get; set; }
    }
}
