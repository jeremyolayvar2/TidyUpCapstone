using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IUserService
    {
        Task<AppUser?> GetUserByIdAsync(string userId);
        Task<decimal> GetUserTokenBalanceAsync(string userId); 
    }
}