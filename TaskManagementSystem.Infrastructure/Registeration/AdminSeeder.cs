using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Infrastructure.Persistance.Data;

namespace TaskManagementSystem.Infrastructure.Registeration
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            // Ensure the database is created
            await context.Database.EnsureCreatedAsync();

            var adminEmail = "admin@example.com";
            
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = adminEmail,
                    PasswordHash = passwordHasher.Hash("Admin@123"),
                    Role = UserRole.Admin,
                    CreatedAT = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedBy = "System",
                    DeletedBy = string.Empty
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
