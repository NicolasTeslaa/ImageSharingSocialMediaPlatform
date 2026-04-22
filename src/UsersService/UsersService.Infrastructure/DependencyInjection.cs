using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsersService.Application.Abstractions;
using UsersService.Domain.Repositories;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Repositories;
using UsersService.Infrastructure.Security;

namespace UsersService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException("Connection string 'UsersDatabase' was not found.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();

        return services;
    }
}
