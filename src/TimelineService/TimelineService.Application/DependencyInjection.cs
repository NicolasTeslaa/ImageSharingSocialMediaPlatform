using Microsoft.Extensions.DependencyInjection;
using TimelineService.Application.Abstractions;
using TimelineService.Application.Services;

namespace TimelineService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTimelineApplication(this IServiceCollection services)
    {
        services.AddScoped<ITimelineQueryService, TimelineQueryService>();
        return services;
    }
}
