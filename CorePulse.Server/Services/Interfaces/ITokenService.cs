using CorePulse.Shared.Models;

namespace CorePulse.Server.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
