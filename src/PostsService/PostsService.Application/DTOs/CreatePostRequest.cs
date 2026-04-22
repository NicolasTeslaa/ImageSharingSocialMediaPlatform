namespace PostsService.Application.DTOs;

public sealed record CreatePostRequest(string PostUrl, string? PostType);
