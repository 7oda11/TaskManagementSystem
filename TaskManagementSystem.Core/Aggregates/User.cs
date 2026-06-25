using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Core.Aggregates
{
    public class User : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
    }
}
