using Blazored.LocalStorage;
using CorePulse.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CorePulse.Client.Identity
{
    /// <summary>
    /// Custom implementation of AuthenticationStateProvider for Blazor WebAssembly.
    /// This class manages the user's authentication state by interacting with browser LocalStorage 
    /// and providing identity claims to the application's components.
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorage = localStorageService;
            // Initialize an anonymous state (user is not logged in)
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        /// <summary>
        /// Retrieves the current authentication state. 
        /// It checks for a stored session and validates if the JWT/Session has not expired.
        /// </summary>
        /// <returns>Authenticated state with user claims or anonymous state if invalid/expired.</returns>
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Attempt to retrieve the session from LocalStorage
            var session = await _localStorage.GetItemAsync<UserSessionDTO>("UserSession");

            // If session doesn't exist or is expired, return anonymous state
            if (session is null || session.Expire < DateTime.UtcNow)
            {
                return _anonymous;
            }

            // Construct the user's claims from the session data
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, session.UserName),
                new Claim(ClaimTypes.Role, session.Role)
            };

            // "JwtAuth" defines the authentication type, marking the identity as authenticated
            var identity = new ClaimsIdentity(claims, "JwtAuth");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }

        /// <summary>
        /// Manually triggers an authentication state update when a user logs in.
        /// Notifies all components (like AuthorizeView) that the user's identity has changed.
        /// </summary>
        /// <param name="sessionDTO">The user session received from the API.</param>
        public void NotifyUserAuthentication(UserSessionDTO sessionDTO)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, sessionDTO.UserName),
                new Claim(ClaimTypes.Role, sessionDTO.Role)
            };

            var identity = new ClaimsIdentity(claims, "JwtAuth");
            var principal = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(principal));

            // Notify Blazor that the authentication state has been updated
            NotifyAuthenticationStateChanged(authState);
        }

        /// <summary>
        /// Manually triggers an authentication state update when a user logs out.
        /// Reverts the application state to anonymous.
        /// </summary>
        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(_anonymous);

            // Notify Blazor that the user is now logged out
            NotifyAuthenticationStateChanged(authState);
        }
    }
}