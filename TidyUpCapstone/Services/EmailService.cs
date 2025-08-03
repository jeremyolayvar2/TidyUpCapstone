using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.DTOs.Configuration; // Add this line

namespace TidyUpCapstone.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridSettingsDto _sendGridSettings; // Updated type name
        private readonly EmailSettingsDto _emailSettings; // Updated type name
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            ISendGridClient sendGridClient,
            IOptions<SendGridSettingsDto> sendGridSettings, // Updated type name
            IOptions<EmailSettingsDto> emailSettings, // Updated type name
            ILogger<EmailService> logger)
        {
            _sendGridClient = sendGridClient;
            _sendGridSettings = sendGridSettings.Value;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        // Keep your existing method signature but implement with SendGrid
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailInternalAsync(email, subject, htmlMessage, htmlMessage);
        }

        public async Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationToken)
        {
            _logger.LogInformation("=== EMAIL SERVICE DEBUG ===");
            _logger.LogInformation("Original token: {Token}", verificationToken.Substring(0, Math.Min(50, verificationToken.Length)) + "...");
            _logger.LogInformation("Original token length: {Length}", verificationToken.Length);

            // ✅ Encode BOTH token and email
            var encodedToken = Uri.EscapeDataString(verificationToken);
            var encodedEmail = Uri.EscapeDataString(toEmail);

            _logger.LogInformation("Encoded token: {Token}", encodedToken.Substring(0, Math.Min(50, encodedToken.Length)) + "...");
            _logger.LogInformation("Encoded token length: {Length}", encodedToken.Length);

            var verificationUrl = $"{_emailSettings.BaseUrl}/Account/ConfirmEmail?token={encodedToken}&email={encodedEmail}";
            _logger.LogInformation("Verification URL: {Url}", verificationUrl.Substring(0, Math.Min(200, verificationUrl.Length)) + "...");

            var subject = "Verify Your TidyUp Account";
            var htmlContent = GetEmailVerificationTemplate(userName, verificationUrl);
            var plainTextContent = $"Hi {userName}, please verify your email by clicking: {verificationUrl}";

            return await SendEmailInternalAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetToken)
        {
            // URL encode BOTH the token and email - CRITICAL!
            var encodedToken = Uri.EscapeDataString(resetToken);
            var encodedEmail = Uri.EscapeDataString(toEmail);

            var resetUrl = $"{_emailSettings.BaseUrl}/Account/ResetPassword?token={encodedToken}&email={encodedEmail}";

            var subject = "Reset Your TidyUp Password";
            var htmlContent = GetPasswordResetTemplate(userName, resetUrl);
            var plainTextContent = $"Hi {userName}, reset your password by clicking: {resetUrl}";

            return await SendEmailInternalAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to TidyUp!";
            var htmlContent = GetWelcomeEmailTemplate(userName);
            var plainTextContent = $"Welcome to TidyUp, {userName}! Your account is now active.";

            return await SendEmailInternalAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendAccountReminderAsync(string toEmail, string userName)
        {
            var loginUrl = $"{_emailSettings.BaseUrl}/Account/Login";

            var subject = "Complete Your TidyUp Registration";
            var htmlContent = GetAccountReminderTemplate(userName, loginUrl);
            var plainTextContent = $"Hi {userName}, don't forget to verify your TidyUp account: {loginUrl}";

            return await SendEmailInternalAsync(toEmail, subject, htmlContent, plainTextContent);
        }

        private async Task<bool> SendEmailInternalAsync(string toEmail, string subject, string htmlContent, string plainTextContent)
        {
            try
            {
                var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to send email to {Email}. Status: {Status}", toEmail, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                return false;
            }
        }

        // Email template methods - we'll create these next
        private string GetEmailVerificationTemplate(string userName, string verificationUrl)
        {
            return $@"
                <h2>Welcome to TidyUp!</h2>
                <p>Hi {userName},</p>
                <p>Thanks for signing up! Please verify your email address by clicking the button below:</p>
                <a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verify Email</a>
                <p>If the button doesn't work, copy and paste this link: {verificationUrl}</p>
                <p>This link will expire in 24 hours.</p>
                <p>Best regards,<br>The TidyUp Team</p>";
        }

        private string GetPasswordResetTemplate(string userName, string resetUrl)
        {
            return $@"
                <h2>Reset Your Password</h2>
                <p>Hi {userName},</p>
                <p>You requested to reset your password. Click the button below to set a new password:</p>
                <a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a>
                <p>If the button doesn't work, copy and paste this link: {resetUrl}</p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>Best regards,<br>The TidyUp Team</p>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
                <h2>Welcome to TidyUp!</h2>
                <p>Hi {userName},</p>
                <p>Your account is now verified and ready to use!</p>
                <p>Start organizing your life with TidyUp today.</p>
                <a href='{_emailSettings.BaseUrl}/Account/Login' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Get Started</a>
                <p>Best regards,<br>The TidyUp Team</p>";
        }

        private string GetAccountReminderTemplate(string userName, string loginUrl)
        {
            return $@"
                <h2>Don't Forget to Complete Your Registration</h2>
                <p>Hi {userName},</p>
                <p>You started creating a TidyUp account but haven't verified your email yet.</p>
                <p>Complete your registration to start using TidyUp:</p>
                <a href='{loginUrl}' style='background-color: #ffc107; color: black; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Complete Registration</a>
                <p>Best regards,<br>The TidyUp Team</p>";
        }
    }
}