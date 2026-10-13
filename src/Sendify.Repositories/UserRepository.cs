using Microsoft.EntityFrameworkCore;
using Sendify.Data;
using Sendify.DataManager;

namespace Sendify.Repositories;

public class UserRepository : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        await using var db = new DataContext();
        
        return await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByUserNameAsync(string userName)
    {
        await using var db = new DataContext();
        
        return await db.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }
}
