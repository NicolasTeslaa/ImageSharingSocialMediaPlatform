namespace SearchService.Application.DTOs;

public sealed record SearchResultDto(Guid Id, string Type, string Title, string Snippet);
