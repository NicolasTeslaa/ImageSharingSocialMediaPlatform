namespace PostsService.Application.DTOs;

public sealed record UpdatePostRequest(string PostUrl, string? PostType);
