using APIGateway.Application.Abstractions;
using APIGateway.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace APIGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayApplication(this IServiceCollection services)
    {
        services.AddScoped<IRouteQueryService, RouteQueryService>();
        return services;
    }
}
