using AutoMapper;
using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all active users.");
            var userRepo = _unitOfWork.Repository<User>();
            var users = await userRepo.GetAllByConditionAsync(u => !u.IsDeleted, cancellationToken);

            return _mapper.Map<IEnumerable<UserProfileDto>>(users);
        }

        public async Task<UserProfileDto> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to create user with email: {Email}", request.Email);
            var userRepo = _unitOfWork.Repository<User>();

            if (await userRepo.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken))
            {
                _logger.LogWarning("User creation failed. Email already exists: {Email}", request.Email);
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role,
                CreatedAT = DateTime.UtcNow,
                CreatedBy = "Admin",
                ModifiedBy = "Admin",
                DeletedBy = string.Empty
            };

            await userRepo.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User created successfully with ID: {UserId}", user.ID);

            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task DeleteUserAsync(int userId, string deletedBy, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to soft delete user with ID: {UserId} by {DeletedBy}", userId, deletedBy);
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByConditionAsync(u => u.ID == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Soft delete failed. User with ID: {UserId} not found.", userId);
                throw new KeyNotFoundException("User not found.");
            }

            user.IsDeleted  = true;
            user.DeletedAt  = DateTime.UtcNow;
            user.DeletedBy  = deletedBy;
            user.ModifiedAt = DateTime.UtcNow;
            user.ModifiedBy = deletedBy;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("User with ID: {UserId} successfully soft deleted.", userId);
        }
    }
}
