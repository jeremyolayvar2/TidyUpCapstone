namespace TidyUpCapstone.Models.DTOs.Configuration
{
    public class SendGridSettingsDto
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class EmailSettingsDto
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TokenExpirationHours { get; set; } = 24;
        public int MaxResendAttempts { get; set; } = 3;
    }
}