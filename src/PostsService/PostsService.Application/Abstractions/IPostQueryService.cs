using PostsService.Application.DTOs;

namespace PostsService.Application.Abstractions;

public interface IPostQueryService
{
    Task<IReadOnlyCollection<PostDto>> GetRecentAsync(CancellationToken cancellationToken = default);
}
