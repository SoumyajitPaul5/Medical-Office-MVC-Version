using System.Net.Mail;

namespace MedicalOffice.ViewModels
{
    /// <summary>
    /// Interface for my own email service
    /// </summary>
    public interface IMyEmailSender
    {
        /// <summary>
        /// Sends an email to a single recipient.
        /// </summary>
        /// <param name="name">The name of the recipient.</param>
        /// <param name="email">The email address of the recipient.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendOneAsync(string name, string email, string subject, string htmlMessage);

        /// <summary>
        /// Sends an email to multiple recipients.
        /// </summary>
        /// <param name="emailMessage">The email message containing the list of recipients, subject, and content.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendToManyAsync(EmailMessage emailMessage);
    }
}
