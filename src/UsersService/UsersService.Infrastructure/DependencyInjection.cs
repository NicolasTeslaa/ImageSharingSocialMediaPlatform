using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UsersService.Application.Abstractions;
using UsersService.Domain.Repositories;
using UsersService.Infrastructure.Options;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Repositories;
using UsersService.Infrastructure.Search;
using UsersService.Infrastructure.Security;

namespace UsersService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException("Connection string 'UsersDatabase' was not found.");

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "JWT secret key is required.")
            .Validate(options => options.SecretKey.Length >= 32, "JWT secret key must be at least 32 characters.")
            .ValidateOnStart();

        services
            .AddOptions<SearchServiceOptions>()
            .Bind(configuration.GetSection(SearchServiceOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Search service base url is required.")
            .ValidateOnStart();

        services.AddDbContext<UsersDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddHttpClient<IUserSearchSyncService, SearchServiceSyncClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<SearchServiceOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
