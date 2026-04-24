using TimelineService.Application.DTOs;

namespace TimelineService.Application.Abstractions;

public interface ITimelineQueryService
{
    Task<IReadOnlyCollection<TimelineItemDto>> GetFeedAsync(Guid userId, CancellationToken cancellationToken = default);
}
