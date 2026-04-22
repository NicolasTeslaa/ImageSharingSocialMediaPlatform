using Microsoft.EntityFrameworkCore;
using PostsService.Application.Abstractions;

namespace PostsService.Infrastructure.Persistence;

public sealed class PostsUnitOfWork(PostsWriteDbContext writeDbContext) : IPostsUnitOfWork
{
    public async Task ExecuteTransactionalAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = writeDbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await writeDbContext.Database.BeginTransactionAsync(cancellationToken);
            await action(cancellationToken);
            await writeDbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
