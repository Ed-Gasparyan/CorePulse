namespace CorePulse.Shared.DTOs.Responses
{
    public class UserSessionDTO
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expire { get; set; }
    }
}
