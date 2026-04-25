using System.ComponentModel.DataAnnotations;

namespace CorePulse.Shared.Models
{
    public enum UserRole
    {
        Admin,
        User
    }
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
