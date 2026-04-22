namespace PostsService.Application.DTOs;

public sealed record PostDto(
    Guid Id,
    Guid UserId,
    string ObjectKey,
    string PostUrl,
    DateTime TimestampUtc,
    string PostType);
