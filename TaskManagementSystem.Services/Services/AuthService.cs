using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var userRepo = _unitOfWork.Repository<User>();

            if (await userRepo.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted))
                throw new InvalidOperationException("A user with this email already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = UserRole.User,
                CreatedAT = DateTime.UtcNow,
                CreatedBy = string.Empty,
                ModifiedBy = string.Empty,
                DeletedBy = string.Empty
            };

            await userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(
                u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return BuildAuthResponse(user);
        }

        public async Task<UserProfileDto?> GetCurrentUserAsync(int userId)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted);

            if (user is null)
                return null;

            return MapToProfile(user);
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var token = _tokenService.GenerateToken(user, out var expiresAt);
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        private static UserProfileDto MapToProfile(User user) => new()
        {
            Id = user.ID,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAT
        };
    }
}
