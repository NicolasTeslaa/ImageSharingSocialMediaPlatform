namespace TimelineService.Domain.Entities;

public sealed record TimelineItem(Guid PostId, Guid UserId, string ImageUrl, DateTime TimestampUtc);
