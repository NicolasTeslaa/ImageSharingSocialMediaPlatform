using TimelineService.Application.DTOs;

namespace TimelineService.Application.Abstractions;

public interface ITimelineQueryService
{
    Task<IReadOnlyCollection<TimelineItemDto>> GetByUserAsync(string userName, CancellationToken cancellationToken = default);
}
