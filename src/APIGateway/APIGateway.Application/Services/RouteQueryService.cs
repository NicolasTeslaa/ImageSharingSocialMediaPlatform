using APIGateway.Application.Abstractions;
using APIGateway.Application.DTOs;
using APIGateway.Domain.Repositories;

namespace APIGateway.Application.Services;

public sealed class RouteQueryService(IServiceRouteRepository routeRepository) : IRouteQueryService
{
    public async Task<IReadOnlyCollection<ServiceRouteDto>> GetRoutesAsync(CancellationToken cancellationToken = default)
    {
        var routes = await routeRepository.GetRoutesAsync(cancellationToken);

        return routes
            .Select(route => new ServiceRouteDto(route.ServiceName, route.BasePath, route.DownstreamUrl))
            .ToArray();
    }
}
