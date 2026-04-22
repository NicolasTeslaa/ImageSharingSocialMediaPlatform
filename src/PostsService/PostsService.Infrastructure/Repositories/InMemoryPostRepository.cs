using PostsService.Domain.Entities;
using PostsService.Domain.Repositories;

namespace PostsService.Infrastructure.Repositories;

public sealed class InMemoryPostRepository : IPostRepository
{
    private static readonly IReadOnlyCollection<Post> Posts =
    [
        new(Guid.NewGuid(), "ana.dev", "Sessao completa publicada com 12 imagens novas.", DateTimeOffset.UtcNow.AddMinutes(-20)),
        new(Guid.NewGuid(), "carla.pix", "Moodboard visual para creators iniciantes.", DateTimeOffset.UtcNow.AddHours(-3)),
        new(Guid.NewGuid(), "bruno.cloud", "Arquitetura do feed desacoplado em producao.", DateTimeOffset.UtcNow.AddHours(-8))
    ];

    public Task<IReadOnlyCollection<Post>> GetRecentAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Posts);
}
