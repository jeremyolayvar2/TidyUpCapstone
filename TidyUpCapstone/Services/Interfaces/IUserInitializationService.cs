using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IUserInitializationService
    {
        Task InitializeUserAsync(int userId);
        Task<bool> IsUserInitializedAsync(int userId);

    }
}