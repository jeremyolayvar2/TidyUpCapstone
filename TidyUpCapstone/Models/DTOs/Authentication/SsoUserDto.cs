namespace TidyUpCapstone.Models.DTOs.Authentication
{
    public class SsoUserDto
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? ProviderDisplayName { get; set; }
        public string? ProviderAvatarUrl { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public string? Scope { get; set; }
    }

    public class SsoProviderDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? AuthorityUrl { get; set; }
        public string? Scopes { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSsoProviderDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? AuthorityUrl { get; set; }
        public string? Scopes { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? ConfigurationJson { get; set; }
    }

    public class UpdateSsoProviderDto
    {
        public string? DisplayName { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AuthorityUrl { get; set; }
        public string? Scopes { get; set; }
        public bool? IsEnabled { get; set; }
        public string? ConfigurationJson { get; set; }
    }

    public class UserSsoLinkDto
    {
        public int LinkId { get; set; }
        public int UserId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? ProviderDisplayName { get; set; }
        public string? ProviderAvatarUrl { get; set; }
        public DateTime LinkedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class SsoLoginRequestDto
    {
        public string Provider { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public string? State { get; set; }
    }

    public class SsoCallbackDto
    {
        public string Provider { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? State { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
    }

    public class SsoTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }
    }
}