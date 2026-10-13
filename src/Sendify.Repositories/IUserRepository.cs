using Sendify.Data;

namespace Sendify.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUserNameAsync(string userName);
}
