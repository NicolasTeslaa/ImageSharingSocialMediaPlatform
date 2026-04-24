using TimelineService.Domain.Entities;

namespace TimelineService.Domain.Repositories;

public interface ITimelineRepository
{
    Task<IReadOnlyCollection<TimelineItem>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TimelineItem>> GetByUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task AddAsync(TimelineItem item, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
