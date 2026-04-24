namespace TimelineService.Application.DTOs;

public sealed record TimelineItemDto(Guid PostId, Guid UserId, string ImageUrl, DateTime TimestampUtc);
