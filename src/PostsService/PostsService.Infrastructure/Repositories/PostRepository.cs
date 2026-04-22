using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Persistence;

namespace PostsService.Infrastructure.Repositories;

public sealed class PostRepository(PostsDbContext dbContext) : IPostRepository
{
    public async Task<IReadOnlyCollection<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Posts
            .AsNoTracking()
            .OrderByDescending(post => post.TimestampUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Posts
            .AsNoTracking()
            .Where(post => post.UserId == userId)
            .OrderByDescending(post => post.TimestampUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Posts.FirstOrDefaultAsync(post => post.Id == id, cancellationToken);
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await dbContext.Posts.AddAsync(post, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        dbContext.Posts.Update(post);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
    {
        dbContext.Posts.Remove(post);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
