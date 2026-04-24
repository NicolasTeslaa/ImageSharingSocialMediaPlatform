using System.Collections.Concurrent;
using TimelineService.Domain.Entities;
using TimelineService.Domain.Repositories;

namespace TimelineService.Infrastructure.Repositories;

public sealed class InMemoryTimelineRepository : ITimelineRepository
{
    private readonly ConcurrentDictionary<Guid, List<TimelineItem>> _timelines = new();

    public Task<IReadOnlyCollection<TimelineItem>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!_timelines.TryGetValue(userId, out var items))
        {
            return Task.FromResult<IReadOnlyCollection<TimelineItem>>([]);
        }

        lock (items)
        {
            return Task.FromResult<IReadOnlyCollection<TimelineItem>>(items.ToArray());
        }
    }

    public Task<IReadOnlyCollection<TimelineItem>> GetByUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var result = new List<TimelineItem>();

        foreach (var userId in userIds.Distinct())
        {
            if (!_timelines.TryGetValue(userId, out var items))
            {
                continue;
            }

            lock (items)
            {
                result.AddRange(items);
            }
        }

        return Task.FromResult<IReadOnlyCollection<TimelineItem>>(result
            .OrderByDescending(item => item.TimestampUtc)
            .ToArray());
    }

    public Task AddAsync(TimelineItem item, CancellationToken cancellationToken = default)
    {
        var timeline = _timelines.GetOrAdd(item.UserId, _ => []);

        lock (timeline)
        {
            timeline.RemoveAll(existing => existing.PostId == item.PostId);

            var insertIndex = timeline.FindIndex(existing => existing.TimestampUtc < item.TimestampUtc);
            if (insertIndex < 0)
            {
                timeline.Add(item);
            }
            else
            {
                timeline.Insert(insertIndex, item);
            }
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _timelines.Clear();
        return Task.CompletedTask;
    }
}
