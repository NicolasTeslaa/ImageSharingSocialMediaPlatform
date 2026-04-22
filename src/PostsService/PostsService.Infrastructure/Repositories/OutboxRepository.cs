using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;
using PostsService.Domain.Enums;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Persistence;

namespace PostsService.Infrastructure.Repositories;

public sealed class OutboxRepository(PostsWriteDbContext writeDbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await writeDbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await writeDbContext.OutboxMessages
            .Where(message => message.Status == OutboxStatus.Pending || message.Status == OutboxStatus.Failed)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await writeDbContext.OutboxMessages
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken);
    }
}
