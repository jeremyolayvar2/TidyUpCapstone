using TidyUpCapstone.Models.Entities;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<decimal> GetUserTokenBalanceAsync(string userId); 
    }
}