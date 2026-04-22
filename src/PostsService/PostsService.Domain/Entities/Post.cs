namespace PostsService.Domain.Entities;

public sealed record Post(Guid Id, string AuthorUserName, string Caption, DateTimeOffset CreatedAt);
