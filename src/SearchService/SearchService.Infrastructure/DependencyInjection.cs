using Microsoft.Extensions.DependencyInjection;
using SearchService.Domain.Repositories;
using SearchService.Infrastructure.Repositories;

namespace SearchService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISearchRepository, InMemorySearchRepository>();
        return services;
    }
}
