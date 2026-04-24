using TimelineService.Application.Abstractions;
using TimelineService.Application.DTOs;
using TimelineService.Domain.Repositories;

namespace TimelineService.Application.Services;

public sealed class TimelineQueryService(
    ITimelineRepository timelineRepository,
    IFollowingLookupService followingLookupService) : ITimelineQueryService
{
    public async Task<IReadOnlyCollection<TimelineItemDto>> GetFeedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var followedUserIds = await followingLookupService.GetFollowingUserIdsAsync(userId, cancellationToken);
        if (followedUserIds.Count == 0)
        {
            return [];
        }

        var items = await timelineRepository.GetByUsersAsync(followedUserIds, cancellationToken);

        return items
            .Select(item => new TimelineItemDto(item.PostId, item.UserId, item.ImageUrl, item.TimestampUtc))
            .ToArray();
    }
}
