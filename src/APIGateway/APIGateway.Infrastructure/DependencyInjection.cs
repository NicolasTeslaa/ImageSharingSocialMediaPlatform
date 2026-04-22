using APIGateway.Domain.Repositories;
using APIGateway.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace APIGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IServiceRouteRepository, InMemoryServiceRouteRepository>();
        return services;
    }
}
