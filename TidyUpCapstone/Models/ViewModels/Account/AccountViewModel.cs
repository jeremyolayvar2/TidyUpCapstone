using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.DTOs.User;
// using TidyUpCapstone.Models.DTOs.Authentication;

namespace TidyUpCapstone.Models.ViewModels.Account
{
    // EXTERNAL LOGIN VIEWMODELS (NEW):

    /// <summary>
    /// ViewModel for the main login page - External providers only
    /// </summary>
    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        // External provider options - only Google and Facebook
        public bool GoogleEnabled { get; set; } = true;
        public bool FacebookEnabled { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for confirming new user registration from external provider
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [Display(Name = "Username (Optional)")]
        public string? Username { get; set; }

        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        // Hidden fields populated from external provider (Google/Facebook)
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderDisplayName { get; set; }
        public string? ProviderAvatarUrl { get; set; }

        // Required agreements
        [Required(ErrorMessage = "You must accept the Terms and Conditions")]
        [Display(Name = "I accept the Terms and Conditions")]
        public bool AcceptTerms { get; set; }

        [Required(ErrorMessage = "You must accept the Privacy Policy")]
        [Display(Name = "I accept the Privacy Policy")]
        public bool AcceptPrivacy { get; set; }

        // Optional preferences
        [Display(Name = "Subscribe to marketing emails")]
        public bool MarketingEmails { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// ViewModel for external login failure scenarios
    /// </summary>
    public class ExternalLoginFailureViewModel
    {
        public string Provider { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ReturnUrl { get; set; }
        public bool CanRetry { get; set; } = true;
    }

    // EXISTING VIEWMODELS (CLEANED FOR EXTERNAL LOGIN):

    public class ProfileViewModel
    {
        public UserProfileDto Profile { get; set; } = new UserProfileDto();
        public UpdateUserProfileDto UpdateProfile { get; set; } = new UpdateUserProfileDto();
        public UserStatsDto Stats { get; set; } = new UserStatsDto();
        public List<string> ConnectedProviders { get; set; } = new List<string>();
        public List<string> AvailableProviders { get; set; } = new List<string>();
        public bool CanEditProfile { get; set; }
        public bool ShowTwoFactorSetup { get; set; }
        public bool ShowEmailVerification { get; set; }
        public bool ShowPhoneVerification { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class SecurityViewModel
    {
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public List<LoginSessionDto> ActiveSessions { get; set; } = new List<LoginSessionDto>();
        public List<LoginLogDto> RecentLogins { get; set; } = new List<LoginLogDto>();
        // REMOVED: ChangePasswordDto - not needed for external login only
        public List<string> ConnectedSsoProviders { get; set; } = new List<string>();
        public DateTime? LastPasswordChange { get; set; } // Keep for audit purposes
        public bool RequirePasswordChange { get; set; } // Keep but will always be false
    }

    public class NotificationSettingsViewModel
    {
        public List<NotificationPreferenceDto> Preferences { get; set; } = new List<NotificationPreferenceDto>();
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool MarketingEmailsEnabled { get; set; }
        public string? NotificationFrequency { get; set; }
        public List<string> NotificationTypes { get; set; } = new List<string>();
    }

    public class LoginSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        public bool IsCurrentSession { get; set; }
    }

    public class LoginLogDto
    {
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string LoginMethod { get; set; } = string.Empty; // Will be "Google" or "Facebook"
        public bool Success { get; set; }
        public string? Location { get; set; }
    }

    // REMOVED: ChangePasswordDto - not needed for external login only

    public class NotificationPreferenceDto
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public string DeliveryMethod { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}