using TimelineService.Domain.Entities;

namespace TimelineService.Domain.Repositories;

public interface ITimelineRepository
{
    Task<IReadOnlyCollection<TimelineItem>> GetByUserAsync(string userName, CancellationToken cancellationToken = default);
}
