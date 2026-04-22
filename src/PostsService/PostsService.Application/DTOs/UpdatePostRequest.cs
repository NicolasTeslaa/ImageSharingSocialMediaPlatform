namespace PostsService.Application.DTOs;

public sealed class UpdatePostRequest
{
    public required Stream FileStream { get; init; }
    public required long FileSize { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; } 
    public string? PostType { get; init; }
}
