//using Microsoft.EntityFrameworkCore;
//using TidyUpCapstone.Models.Entities.User;
//using TidyUpCapstone.Services.Interfaces;
//using TidyUpCapstone.Data;

//namespace TidyUpCapstone.Services
//{
//    public class UserService : IUserService
//    {
//        private readonly ApplicationDbContext _context;

//        public UserService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        //public async Task<AppUser?> GetUserByIdAsync(string userId)
//        //{
//        //    return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
//        //}

//        //public async Task<decimal> GetUserTokenBalanceAsync(string userId)
//        //{
//        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
//        //    return user?.TokenBalance ?? 0;
//        //}
//    }

//}