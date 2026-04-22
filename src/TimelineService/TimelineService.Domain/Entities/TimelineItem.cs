namespace TimelineService.Domain.Entities;

public sealed record TimelineItem(Guid Id, string UserName, string ContentPreview, DateTimeOffset PublishedAt);
