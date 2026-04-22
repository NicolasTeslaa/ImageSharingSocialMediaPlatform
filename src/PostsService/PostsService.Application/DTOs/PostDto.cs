namespace PostsService.Application.DTOs;

public sealed record PostDto(Guid Id, string AuthorUserName, string Caption, DateTimeOffset CreatedAt);
