using PostsService.Domain.Entities;

namespace PostsService.Domain.Repositories;

public interface IPostRepository
{
    Task<IReadOnlyCollection<Post>> GetRecentAsync(CancellationToken cancellationToken = default);
}
