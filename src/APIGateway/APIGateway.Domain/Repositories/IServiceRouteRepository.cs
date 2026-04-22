using APIGateway.Domain.Entities;

namespace APIGateway.Domain.Repositories;

public interface IServiceRouteRepository
{
    Task<IReadOnlyCollection<ServiceRoute>> GetRoutesAsync(CancellationToken cancellationToken = default);
}
