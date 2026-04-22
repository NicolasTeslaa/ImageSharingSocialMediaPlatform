using PostsService.Domain.Enums;

namespace PostsService.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string type,
        string aggregateType,
        Guid aggregateId,
        string payload,
        DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        Status = OutboxStatus.Pending;
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? PublishedOnUtc { get; private set; }
    public OutboxStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public static OutboxMessage Create(
        string type,
        string aggregateType,
        Guid aggregateId,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(aggregateType))
        {
            throw new ArgumentException("Outbox aggregate type is required.", nameof(aggregateType));
        }

        if (aggregateId == Guid.Empty)
        {
            throw new ArgumentException("Outbox aggregate id is required.", nameof(aggregateId));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Outbox payload is required.", nameof(payload));
        }

        return new OutboxMessage(
            Guid.NewGuid(),
            type.Trim(),
            aggregateType.Trim(),
            aggregateId,
            payload,
            DateTime.UtcNow);
    }

    public void MarkPublished()
    {
        Status = OutboxStatus.Published;
        PublishedOnUtc = DateTime.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Attempts += 1;
        Status = OutboxStatus.Failed;
        LastError = string.IsNullOrWhiteSpace(error) ? "Unknown Kafka publishing error." : error;
    }

    public void ResetForRetry()
    {
        Status = OutboxStatus.Pending;
    }
}
