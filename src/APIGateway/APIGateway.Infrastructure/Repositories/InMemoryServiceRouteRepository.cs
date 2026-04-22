using APIGateway.Domain.Entities;
using APIGateway.Domain.Repositories;

namespace APIGateway.Infrastructure.Repositories;

public sealed class InMemoryServiceRouteRepository : IServiceRouteRepository
{
    private static readonly IReadOnlyCollection<ServiceRoute> Routes =
    [
        new("UsersService", "/users", "https://localhost:7001"),
        new("SearchService", "/search", "https://localhost:7002"),
        new("TimelineService", "/timeline", "https://localhost:7003"),
        new("PostsService", "/posts", "https://localhost:7004")
    ];

    public Task<IReadOnlyCollection<ServiceRoute>> GetRoutesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Routes);
}
