using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace MedicalOffice.Utilities
{
    // Class to handle email sending
    public class EmailSender : IEmailSender
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailSender> _logger;

        // Constructor to initialize email configuration and logger
        public EmailSender(IEmailConfiguration emailConfiguration, ILogger<EmailSender> logger)
        {
            _emailConfiguration = emailConfiguration;
            _logger = logger;
        }

        // Method to send an email asynchronously
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Create a new email message
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(email, email));
            message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUsername));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            try
            {
                using var emailClient = new SmtpClient();
                // Connect to the SMTP server
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);

                // Remove XOAUTH2 authentication mechanism
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                // Authenticate with the SMTP server
                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                // Send the email
                await emailClient.SendAsync(message);

                // Disconnect from the SMTP server
                emailClient.Disconnect(true);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex.GetBaseException().Message);
            }
        }
    }
}
