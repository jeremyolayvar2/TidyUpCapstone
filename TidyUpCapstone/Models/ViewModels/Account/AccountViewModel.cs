using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.DTOs.User;
using TidyUpCapstone.Models.DTOs.Authentication;

namespace TidyUpCapstone.Models.ViewModels.Account
{
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

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
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
