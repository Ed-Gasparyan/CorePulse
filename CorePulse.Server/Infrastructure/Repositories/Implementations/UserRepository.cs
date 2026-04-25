using CorePulse.Server.Infrastructure.Repositories.Interfaces;
using CorePulse.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CorePulse.Server.Infrastructure.Repositories.Implementations
{
    /// <summary>
    /// Implementation of the Repository pattern for User entity operations.
    /// This layer abstracts the database access logic from the business services.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously adds a new user to the database tracking context.
        /// </summary>
        /// <param name="user">The User entity to be added.</param>
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <summary>
        /// Retrieves a user from the database based on their email address.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>A User entity if found; otherwise, null.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// Retrieves a user from the database based on their username.
        /// </summary>
        /// <param name="userName">The username to search for.</param>
        /// <returns>A User entity if found; otherwise, null.</returns>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
        }

        /// <summary>
        /// Persists all tracked changes in the context to the underlying SQL database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}