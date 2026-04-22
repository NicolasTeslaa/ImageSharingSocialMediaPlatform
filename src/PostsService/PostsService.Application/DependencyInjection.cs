using Microsoft.Extensions.DependencyInjection;
using PostsService.Application.Abstractions;
using PostsService.Application.Services;

namespace PostsService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsApplication(this IServiceCollection services)
    {
        services.AddScoped<IPostQueryService, PostQueryService>();
        return services;
    }
}
