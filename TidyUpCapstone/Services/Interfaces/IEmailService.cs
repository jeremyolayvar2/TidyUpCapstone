using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IEmailService
    {
        // Keep your existing method
        Task SendEmailAsync(string email, string subject, string htmlMessage);

        // Add new methods for registration system
        Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationToken);
        Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendAccountReminderAsync(string toEmail, string userName);
    }
}