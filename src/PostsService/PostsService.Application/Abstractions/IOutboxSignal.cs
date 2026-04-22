namespace PostsService.Application.Abstractions;

public interface IOutboxSignal
{
    ValueTask SignalAsync(Guid outboxMessageId, CancellationToken cancellationToken = default);
}
