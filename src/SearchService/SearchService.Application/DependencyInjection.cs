using Microsoft.Extensions.DependencyInjection;
using SearchService.Application.Abstractions;
using SearchService.Application.Services;

namespace SearchService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchApplication(this IServiceCollection services)
    {
        services.AddScoped<ISearchQueryService, SearchQueryService>();
        services.AddScoped<ISearchIndexService, SearchIndexService>();
        return services;
    }
}
