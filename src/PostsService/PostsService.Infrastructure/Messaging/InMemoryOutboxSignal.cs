using System.Threading.Channels;
using PostsService.Application.Abstractions;

namespace PostsService.Infrastructure.Messaging;

public sealed class InMemoryOutboxSignal : IOutboxSignal
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask SignalAsync(Guid outboxMessageId, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(outboxMessageId, cancellationToken);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
