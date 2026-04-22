using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Persistence;

namespace PostsService.Infrastructure.Repositories;

public sealed class PostRepository(
    PostsReadDbContext readDbContext,
    PostsWriteDbContext writeDbContext) : IPostRepository
{
    public async Task<IReadOnlyCollection<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await readDbContext.Posts
            .AsNoTracking()
            .OrderByDescending(post => post.TimestampUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await readDbContext.Posts
            .AsNoTracking()
            .Where(post => post.UserId == userId)
            .OrderByDescending(post => post.TimestampUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await readDbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Id == id, cancellationToken);
    }

    public async Task<Post?> GetByIdForWriteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await writeDbContext.Posts.FirstOrDefaultAsync(post => post.Id == id, cancellationToken);
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await writeDbContext.Posts.AddAsync(post, cancellationToken);
    }

    public Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        writeDbContext.Posts.Update(post);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
    {
        writeDbContext.Posts.Remove(post);
        return Task.CompletedTask;
    }
}
