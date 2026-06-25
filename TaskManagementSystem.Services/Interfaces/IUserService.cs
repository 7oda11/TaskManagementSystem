using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserProfileDto> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int userId, string deletedBy, CancellationToken cancellationToken = default);
    }
}
