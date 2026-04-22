using Microsoft.Extensions.DependencyInjection;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Repositories;

namespace PostsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPostRepository, InMemoryPostRepository>();
        return services;
    }
}
