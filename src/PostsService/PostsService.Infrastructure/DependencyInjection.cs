using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Persistence;
using PostsService.Infrastructure.Repositories;
using PostsService.Infrastructure.Security;

namespace PostsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostsDatabase")
            ?? throw new InvalidOperationException("Connection string 'PostsDatabase' was not found.");

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "JWT secret key is required.")
            .Validate(options => options.SecretKey.Length >= 32, "JWT secret key must be at least 32 characters.")
            .ValidateOnStart();

        services.AddDbContext<PostsDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IPostRepository, PostRepository>();

        return services;
    }
}
