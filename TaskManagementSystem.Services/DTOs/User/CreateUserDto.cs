using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Services.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;
    }
}
