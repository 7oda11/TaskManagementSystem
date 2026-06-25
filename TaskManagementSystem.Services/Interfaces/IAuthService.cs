using TaskManagementSystem.Services.DTOs.Auth;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserProfileDto?> GetCurrentUserAsync(int userId);
    }
}
