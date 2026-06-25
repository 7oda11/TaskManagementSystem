using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.User;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync()
        {
            var userRepo = _unitOfWork.Repository<User>();
            var users = await userRepo.GetAllAsync();

            return users.Where(u => !u.IsDeleted).Select(u => new UserProfileDto
            {
                Id = u.ID,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAT
            });
        }

        public async Task<UserProfileDto> CreateUserAsync(CreateUserDto request)
        {
            var userRepo = _unitOfWork.Repository<User>();

            if (await userRepo.ExistsAsync(u => u.Email == request.Email && !u.IsDeleted))
                throw new InvalidOperationException("A user with this email already exists.");

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

            await userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new UserProfileDto
            {
                Id = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAT
            };
        }

        public async Task DeleteUserAsync(int userId)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(userId);

            if (user == null || user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            user.IsDeleted = true;
            user.DeletedBy = "Admin";
            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
