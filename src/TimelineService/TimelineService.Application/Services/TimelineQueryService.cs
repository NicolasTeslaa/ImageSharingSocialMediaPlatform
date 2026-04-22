using TimelineService.Application.Abstractions;
using TimelineService.Application.DTOs;
using TimelineService.Domain.Repositories;

namespace TimelineService.Application.Services;

public sealed class TimelineQueryService(ITimelineRepository timelineRepository) : ITimelineQueryService
{
    public async Task<IReadOnlyCollection<TimelineItemDto>> GetByUserAsync(string userName, CancellationToken cancellationToken = default)
    {
        var items = await timelineRepository.GetByUserAsync(userName, cancellationToken);

        return items
            .Select(item => new TimelineItemDto(item.Id, item.UserName, item.ContentPreview, item.PublishedAt))
            .ToArray();
    }
}
