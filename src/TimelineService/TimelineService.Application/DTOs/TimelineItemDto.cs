namespace TimelineService.Application.DTOs;

public sealed record TimelineItemDto(Guid Id, string UserName, string ContentPreview, DateTimeOffset PublishedAt);
