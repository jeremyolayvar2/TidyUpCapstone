using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Services
{
    public interface IPrivacyService
    {
        Task<UserPrivacySettings> GetUserPrivacySettingsAsync(int userId);
        Task<bool> CanViewProfile(int viewerId, int targetUserId);
        Task<bool> CanViewContactInfo(int viewerId, int targetUserId);
        Task<bool> CanViewActivityHistory(int viewerId, int targetUserId);
        Task<bool> CanViewOnlineStatus(int viewerId, int targetUserId);
        Task<bool> CanViewActivityStreaks(int viewerId, int targetUserId);
        Task<bool> CanViewLocation(int viewerId, int targetUserId);
        Task<bool> ShouldIndexProfile(int userId);
    }

    public class PrivacyService : IPrivacyService
    {
        private readonly ApplicationDbContext _context;

        public PrivacyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserPrivacySettings> GetUserPrivacySettingsAsync(int userId)
        {
            return await _context.UserPrivacySettings
                .FirstOrDefaultAsync(p => p.UserId == userId)
                ?? new UserPrivacySettings { UserId = userId };
        }

        public async Task<bool> CanViewProfile(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.ProfileVisibility == "public";
        }

        public async Task<bool> CanViewContactInfo(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.ContactVisibility == "public";
        }

        public async Task<bool> CanViewActivityHistory(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.ActivityHistory == "show";
        }

        public async Task<bool> CanViewOnlineStatus(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.OnlineStatus == "show";
        }

        public async Task<bool> CanViewActivityStreaks(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.ActivityStreaksVisibility == "show";
        }

        public async Task<bool> CanViewLocation(int viewerId, int targetUserId)
        {
            if (viewerId == targetUserId) return true;
            var settings = await GetUserPrivacySettingsAsync(targetUserId);
            return settings.LocationVisibility == "show";
        }

        public async Task<bool> ShouldIndexProfile(int userId)
        {
            var settings = await GetUserPrivacySettingsAsync(userId);
            return settings.SearchIndexing == "allow";
        }
    }
}