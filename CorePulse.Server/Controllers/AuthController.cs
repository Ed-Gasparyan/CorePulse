using CorePulse.Server.Services.Interfaces;
using CorePulse.Shared.DTOs;
using CorePulse.Shared.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CorePulse.Server.Controllers
{
    /// <summary>
    /// API Controller for managing user authentication and account creation.
    /// Provides endpoints for secure login and registration.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Routes requests to 'api/auth'
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The registration details (Username, Email, Password).</param>
        /// <returns>
        /// 200 OK if successful, 
        /// 400 Bad Request if the username or email is already in use.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result)
            {
                // Returns 400 status to indicate a client-side data conflict
                return BadRequest("Username or Email is already taken.");
            }

            return Ok("Registration successful.");
        }

        /// <summary>
        /// Authenticates a user and starts a new session.
        /// </summary>
        /// <param name="request">Login credentials (Email and Password).</param>
        /// <returns>
        /// 200 OK with UserSessionDTO (JWT token) if successful, 
        /// 401 Unauthorized if credentials fail validation.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var session = await _authService.LoginAsync(request);

            if (session == null)
            {
                // Returns 401 status for invalid credentials (Security best practice)
                return Unauthorized("Invalid email or password.");
            }

            // Return the session object including the generated JWT token
            return Ok(session);
        }
    }
}