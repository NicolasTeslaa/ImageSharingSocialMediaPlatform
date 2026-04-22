using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PostsService.Infrastructure.Options;

namespace PostsService.Infrastructure.Storage;

public sealed class ObjectStorageBucketInitializerHostedService(
    IMinioClient minioClient,
    IOptions<ObjectStorageOptions> objectStorageOptions,
    ILogger<ObjectStorageBucketInitializerHostedService> logger) : IHostedService
{
    private readonly ObjectStorageOptions _options = objectStorageOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            cancellationToken);

        if (exists)
        {
            return;
        }

        await minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(_options.BucketName),
            cancellationToken);

        logger.LogInformation("Created object storage bucket {BucketName}.", _options.BucketName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
