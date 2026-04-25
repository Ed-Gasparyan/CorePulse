using CorePulse.Server.Services.Interfaces;
using CorePulse.Shared.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CorePulse.Server.Services.Implementations
{
    /// <summary>
    /// Service for generating JSON Web Tokens (JWT) for authenticated users.
    /// Encapsulates all security configurations such as signing keys and claims.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Retrieve security key from configuration and encode it for HMAC encryption
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
        }

        /// <summary>
        /// Creates a signed JWT token containing user identity and role claims.
        /// </summary>
        /// <param name="user">The user for whom the token is being generated.</param>
        /// <returns>A string representing the encoded JWT token.</returns>
        public string CreateToken(User user)
        {
            // Define identity claims (metadata embedded in the token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Critical for role-based authorization in Blazor
            };

            // Specify the hashing algorithm and key for the digital signature
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            // Configure token parameters including expiration (7 days)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = credentials,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the serialized token string
            return tokenHandler.WriteToken(token);
        }
    }
}