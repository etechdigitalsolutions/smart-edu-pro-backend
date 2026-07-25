using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartEduPro.Application.Common.Interfaces;
using SmartEduPro.Infrastructure.Persistence;
using SmartEduPro.Infrastructure.Persistence.Repositories;

namespace SmartEduPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenService, Services.JwtTokenService>();
        services.AddScoped<IPasswordHasher, Services.PasswordHasher>();
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();

        return services;
    }
}
