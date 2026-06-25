using TaskManagementSystem.Core.Aggregates;

namespace TaskManagementSystem.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, out DateTime expiresAt);
    }
}
