using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Attempting to register new user with email: {Email}", request.Email);
            var userRepo = _unitOfWork.Repository<User>();

            if (await userRepo.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted))
            {
                _logger.LogWarning("Registration failed. User with email {Email} already exists.", request.Email);
                throw new InvalidOperationException("A user with this email already exists.");
            }

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

            _logger.LogInformation("Successfully registered user with ID: {UserId}", user.ID);

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(
                u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            _logger.LogInformation("User logged in successfully: {UserId}", user.ID);
            return BuildAuthResponse(user);
        }

        public async Task<UserProfileDto?> GetCurrentUserAsync(int userId)
        {
            _logger.LogInformation("Retrieving current user profile for ID: {UserId}", userId);
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted);

            if (user is null)
            {
                _logger.LogWarning("User profile retrieval failed. ID: {UserId} not found.", userId);
                return null;
            }

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
