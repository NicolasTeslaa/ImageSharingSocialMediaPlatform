using APIGateway.Domain.Entities;
using APIGateway.Domain.Repositories;

namespace APIGateway.Infrastructure.Repositories;

public sealed class InMemoryServiceRouteRepository : IServiceRouteRepository
{
    private static readonly IReadOnlyCollection<ServiceRoute> Routes =
    [
        new("UsersService", "/users", "http://localhost:5166"),
        new("UsersService", "/auth", "http://localhost:5166"),
        new("SearchService", "/search", "http://localhost:5239"),
        new("TimelineService", "/timeline", "http://localhost:5174"),
        new("PostsService", "/posts", "http://localhost:5237")
    ];

    public Task<IReadOnlyCollection<ServiceRoute>> GetRoutesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Routes);
}
