using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistance;
using TaskManagementSystem.Infrastructure.Persistance.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.Infrastructure.Registeration
{
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<ApplicationDBContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("Default")));

            services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();

            return services;
        }
    }
}
