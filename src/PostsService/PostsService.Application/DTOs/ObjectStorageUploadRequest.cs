namespace PostsService.Application.DTOs;

public sealed class ObjectStorageUploadRequest
{
    public required Stream FileStream { get; init; }
    public required long FileSize { get; init; }
    public required string ObjectKey { get; init; }
    public required string ContentType { get; init; }
}
