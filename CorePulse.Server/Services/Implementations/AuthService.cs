using CorePulse.Server.Infrastructure.Repositories.Interfaces;
using CorePulse.Server.Services.Interfaces;
using CorePulse.Shared.DTOs;
using CorePulse.Shared.DTOs.Requests;
using CorePulse.Shared.DTOs.Responses;
using CorePulse.Shared.Models;

namespace CorePulse.Server.Services.Implementations
{
    /// <summary>
    /// Service responsible for handling user authentication and registration logic.
    /// It acts as a bridge between the Controllers and the Data Access Layer (Repositories).
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Validates user credentials and generates a secure JWT session.
        /// </summary>
        /// <param name="request">Contains login email and raw password.</param>
        /// <returns>A UserSessionDTO containing the JWT token and user info, or null if validation fails.</returns>
        public async Task<UserSessionDTO?> LoginAsync(LoginRequestDTO request)
        {
            // Retrieve user by email from the database
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Verify if user exists and compare the provided password with the stored secure hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            // Generate a JWT token using the specialized TokenService
            var token = _tokenService.CreateToken(user);

            // Map the user data and token to a session object
            return new UserSessionDTO
            {
                Token = token,
                UserName = user.UserName,
                Role = user.Role.ToString(),
                Expire = DateTime.UtcNow.AddDays(7) // Session lifespan
            };
        }

        /// <summary>
        /// Handles new user registration with duplicate checks and password hashing.
        /// </summary>
        /// <param name="request">Contains new user details (Username, Email, Password).</param>
        /// <returns>True if registration is successful, False if username or email is already taken.</returns>
        public async Task<bool> RegisterAsync(RegisterRequestDTO request)
        {
            // Check for duplicate username to ensure identity uniqueness
            var userNameExists = await _userRepository.GetByUserNameAsync(request.UserName);
            if (userNameExists != null)
            {
                return false;
            }

            // Check if the email is already registered in the system
            var emailExists = await _userRepository.GetByEmailAsync(request.Email);
            if (emailExists != null)
            {
                return false;
            }

            // Create a new User entity and hash the password before saving (Never store raw passwords!)
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.User // Default role for new sign-ups
            };

            // Save the new user to the database
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}