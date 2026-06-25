using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Services.Interfaces;
using TaskManagementSystem.Services.Services;

namespace TaskManagementSystem.Services.Registeration
{
    public static class ServicesServiceExtension
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
