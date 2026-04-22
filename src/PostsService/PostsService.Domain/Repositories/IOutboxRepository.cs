using PostsService.Domain.Entities;

namespace PostsService.Domain.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
