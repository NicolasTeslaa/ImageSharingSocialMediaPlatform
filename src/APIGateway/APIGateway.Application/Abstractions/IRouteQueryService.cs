using APIGateway.Application.DTOs;

namespace APIGateway.Application.Abstractions;

public interface IRouteQueryService
{
    Task<IReadOnlyCollection<ServiceRouteDto>> GetRoutesAsync(CancellationToken cancellationToken = default);
}
