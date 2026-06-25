using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
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
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to register new user with email: {Email}", request.Email);
            var userRepo = _unitOfWork.Repository<User>();

            if (await userRepo.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken))
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

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days lifespan

            await userRepo.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully registered user with ID: {UserId}", user.ID);

            return BuildAuthResponse(user, refreshToken);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(
                u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted, cancellationToken);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged in successfully: {UserId}", user.ID);
            return BuildAuthResponse(user, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid access token.");

            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted, cancellationToken);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token for User ID: {UserId}", userId);
                throw new UnauthorizedAccessException("Invalid client request");
            }

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            
            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BuildAuthResponse(user, newRefreshToken);
        }

        public async Task<UserProfileDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving current user profile for ID: {UserId}", userId);
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User profile retrieval failed. ID: {UserId} not found.", userId);
                return null;
            }

            return _mapper.Map<UserProfileDto>(user);
        }

        private AuthResponseDto BuildAuthResponse(User user, string refreshToken)
        {
            var token = _tokenService.GenerateToken(user, out var expiresAt);
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
        }
    }
}
