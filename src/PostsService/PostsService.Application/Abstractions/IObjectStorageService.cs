using PostsService.Application.DTOs;

namespace PostsService.Application.Abstractions;

public interface IObjectStorageService
{
    Task<ObjectStorageUploadResult> UploadAsync(ObjectStorageUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
