namespace PostsService.Application.DTOs;

public sealed record PostCreatedIntegrationEvent(
    Guid EventId,
    Guid PostId,
    Guid UserId,
    string ObjectKey,
    string PostUrl,
    string PostType,
    DateTime TimestampUtc,
    DateTime OccurredOnUtc);
