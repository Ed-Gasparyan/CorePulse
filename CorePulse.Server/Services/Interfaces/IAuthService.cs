using CorePulse.Shared.DTOs;
using CorePulse.Shared.DTOs.Requests;
using CorePulse.Shared.DTOs.Responses;

namespace CorePulse.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserSessionDTO?> LoginAsync(LoginRequestDTO request);

        Task<bool> RegisterAsync(RegisterRequestDTO request);
    }
}
