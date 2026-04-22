namespace PostsService.Infrastructure.Options;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    public string Endpoint { get; init; } = string.Empty;
    public string PublicEndpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "posts";
    public bool UseSsl { get; init; }
}
