using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileDto>> GetAllUsersAsync();
        Task<UserProfileDto> CreateUserAsync(CreateUserDto request);
        Task DeleteUserAsync(int userId);
    }
}
