using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Helpers
{
    /// <summary>
    /// Helper class for user-related operations like avatar handling
    /// </summary>
    public static class UserHelper
    {
        /// <summary>
        /// Gets the user's avatar URL with proper formatting and cache busting
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="addCacheBusting">Whether to add cache busting parameter (default: true)</param>
        /// <returns>Formatted avatar URL or default avatar path</returns>
        public static string GetUserAvatarUrl(AppUser? user, bool addCacheBusting = true)
        {
            if (user == null || string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                return "/assets/default-avatar.svg";
            }

            // Ensure the path starts with / for proper web path
            var avatarPath = user.ProfilePictureUrl.StartsWith("/")
                ? user.ProfilePictureUrl
                : "/" + user.ProfilePictureUrl;

            // Add cache busting for immediate updates
            if (addCacheBusting)
            {
                return $"{avatarPath}?v={DateTime.UtcNow.Ticks}";
            }

            return avatarPath;
        }

        /// <summary>
        /// Gets user's full display name (First + Last name with fallback to username)
        /// </summary>
        /// <param name="user">The user object</param>
        /// <returns>Formatted display name</returns>
        public static string GetUserDisplayName(AppUser? user)
        {
            if (user == null)
                return "Unknown User";

            // Try to use first and last name
            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                return $"{user.FirstName} {user.LastName}";
            }

            // Fallback to first name only
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                return user.FirstName;
            }

            // Fallback to username
            if (!string.IsNullOrEmpty(user.UserName))
            {
                return user.UserName;
            }

            return "Unknown User";
        }

        /// <summary>
        /// Gets user's username with @ prefix for display
        /// </summary>
        /// <param name="user">The user object</param>
        /// <returns>Username with @ prefix</returns>
        public static string GetUserHandle(AppUser? user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
                return "@unknown";

            return $"@{user.UserName}";
        }

        /// <summary>
        /// Validates if the user has a complete profile
        /// </summary>
        /// <param name="user">The user object</param>
        /// <returns>True if profile is complete</returns>
        public static bool IsProfileComplete(AppUser? user)
        {
            if (user == null)
                return false;

            return !string.IsNullOrEmpty(user.FirstName) &&
                   !string.IsNullOrEmpty(user.LastName) &&
                   !string.IsNullOrEmpty(user.Email) &&
                   user.EmailConfirmed;
        }

        /// <summary>
        /// Gets a safe file name for profile picture uploads
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="originalFileName">Original file name</param>
        /// <returns>Safe file name with user ID and timestamp</returns>
        public static string GenerateProfilePictureFileName(int userId, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{userId}_{timestamp}_{Guid.NewGuid().ToString("N")[..8]}{extension}";
        }
    }
}