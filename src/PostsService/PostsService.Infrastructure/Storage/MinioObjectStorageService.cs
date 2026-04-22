using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Infrastructure.Options;

namespace PostsService.Infrastructure.Storage;

public sealed class MinioObjectStorageService(
    IMinioClient minioClient,
    IOptions<ObjectStorageOptions> objectStorageOptions) : IObjectStorageService
{
    private readonly ObjectStorageOptions _options = objectStorageOptions.Value;

    public async Task<ObjectStorageUploadResult> UploadAsync(ObjectStorageUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FileStream.CanSeek)
        {
            request.FileStream.Position = 0;
        }

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(request.ObjectKey)
                .WithStreamData(request.FileStream)
                .WithObjectSize(request.FileSize)
                .WithContentType(request.ContentType),
            cancellationToken);

        return new ObjectStorageUploadResult(request.ObjectKey, BuildPublicUrl(request.ObjectKey));
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return;
        }

        await minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey),
            cancellationToken);
    }

    private string BuildPublicUrl(string objectKey)
    {
        var baseUrl = _options.PublicEndpoint.TrimEnd('/');
        return $"{baseUrl}/{_options.BucketName}/{objectKey}";
    }
}
