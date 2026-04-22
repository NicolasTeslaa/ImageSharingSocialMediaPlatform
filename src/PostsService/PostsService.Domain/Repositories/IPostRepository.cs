using PostsService.Domain.Entities;

namespace PostsService.Domain.Repositories;

public interface IPostRepository
{
    Task<IReadOnlyCollection<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> GetByIdForWriteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}
