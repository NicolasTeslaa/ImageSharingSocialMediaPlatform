namespace TimelineService.Application.Abstractions;

public interface IFollowingLookupService
{
    Task<IReadOnlyCollection<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}
