namespace SearchService.Domain.Entities;

public sealed record SearchResult(Guid Id, string Type, string Title, string Snippet);
