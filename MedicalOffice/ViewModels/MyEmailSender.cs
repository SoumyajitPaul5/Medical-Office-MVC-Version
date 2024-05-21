using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace MedicalOffice.ViewModels
{
    public class MyEmailSender : IMyEmailSender
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public MyEmailSender(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        /// <summary>
        /// Asynchronously builds and sends a message to a single recipient
        /// </summary>        
        /// <param name="name">Optional - Uses the email if not supplied</param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>
        public async Task SendOneAsync(string name, string email, string subject, string htmlMessage)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = email;
            }
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(name, email));
            message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUsername));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var emailClient = new SmtpClient();
            emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

            await emailClient.SendAsync(message);

            emailClient.Disconnect(true);
        }

        /// <summary>
        /// Asynchronously sends a message to a List of email addresses
        /// </summary>
        /// <param name="emailMessage"></param>
        /// <returns></returns>
        public async Task SendToManyAsync(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUsername));

            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            using var emailClient = new SmtpClient();
            emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

            await emailClient.SendAsync(message);

            emailClient.Disconnect(true);
        }
    }

}
