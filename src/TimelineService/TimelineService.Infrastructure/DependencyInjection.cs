using Microsoft.Extensions.DependencyInjection;
using TimelineService.Domain.Repositories;
using TimelineService.Infrastructure.Repositories;

namespace TimelineService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTimelineInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITimelineRepository, InMemoryTimelineRepository>();
        return services;
    }
}
