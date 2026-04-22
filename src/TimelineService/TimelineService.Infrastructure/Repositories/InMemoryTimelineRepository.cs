using TimelineService.Domain.Entities;
using TimelineService.Domain.Repositories;

namespace TimelineService.Infrastructure.Repositories;

public sealed class InMemoryTimelineRepository : ITimelineRepository
{
    private static readonly IReadOnlyCollection<TimelineItem> Items =
    [
        new(Guid.NewGuid(), "ana.dev", "Novo album de retratos urbanos publicado.", DateTimeOffset.UtcNow.AddMinutes(-15)),
        new(Guid.NewGuid(), "ana.dev", "Bastidores da sessao de fotos com luz natural.", DateTimeOffset.UtcNow.AddHours(-2)),
        new(Guid.NewGuid(), "bruno.cloud", "Insights sobre feed distribuido e ranking.", DateTimeOffset.UtcNow.AddHours(-5))
    ];

    public Task<IReadOnlyCollection<TimelineItem>> GetByUserAsync(string userName, CancellationToken cancellationToken = default)
    {
        var filtered = string.IsNullOrWhiteSpace(userName)
            ? Items
            : Items.Where(item => item.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)).ToArray();

        return Task.FromResult(filtered);
    }
}
