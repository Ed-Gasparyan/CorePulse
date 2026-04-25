using CorePulse.Shared.Models;

namespace CorePulse.Server.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
