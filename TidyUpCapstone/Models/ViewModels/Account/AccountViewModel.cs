using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.DTOs.User;
using TidyUpCapstone.Models.DTOs.Authentication;

namespace TidyUpCapstone.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public bool GoogleEnabled { get; set; } = true;
        public bool FacebookEnabled { get; set; } = false;
    }

    /// <summary>
    /// ViewModel for external login confirmation
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the Terms of Service")]
        public bool AcceptTerms { get; set; } = false;

        [Required(ErrorMessage = "You must accept the Privacy Policy")]
        public bool AcceptPrivacy { get; set; } = false;

        public bool MarketingEmails { get; set; } = false;

        // OAuth provider information
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderDisplayName { get; set; }
        public string? ProviderAvatarUrl { get; set; } 
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
        public ChangePasswordDto ChangePassword { get; set; } = new ChangePasswordDto();
        public List<string> ConnectedSsoProviders { get; set; } = new List<string>();
        public DateTime? LastPasswordChange { get; set; }
        public bool RequirePasswordChange { get; set; }
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
        public string LoginMethod { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Location { get; set; }
    }

    /// <summary>
    /// DTO for changing user password in Settings
    /// This is the CORRECT version that matches your existing implementation
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

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