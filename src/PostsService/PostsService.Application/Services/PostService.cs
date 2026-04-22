using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using System.Text.Json;
using PostsService.Domain.Entities;
using PostsService.Domain.Enums;
using PostsService.Domain.Repositories;

namespace PostsService.Application.Services;

public sealed class PostService(
    IPostRepository postRepository,
    IOutboxRepository outboxRepository,
    IOutboxSignal outboxSignal,
    IObjectStorageService objectStorageService,
    IPostsUnitOfWork unitOfWork) : IPostService
{
    public async Task<IReadOnlyCollection<PostDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var posts = await postRepository.GetAllAsync(cancellationToken);
        return posts.Select(MapToDto).ToArray();
    }

    public async Task<IReadOnlyCollection<PostDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var posts = await postRepository.GetByUserIdAsync(userId, cancellationToken);
        return posts.Select(MapToDto).ToArray();
    }

    public async Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetByIdAsync(id, cancellationToken);
        return post is null ? null : MapToDto(post);
    }

    public async Task<PostDto> CreateAsync(Guid authenticatedUserId, CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        ValidateFileRequest(request.FileStream, request.FileSize, request.FileName);

        var postId = Guid.NewGuid();
        var objectKey = BuildObjectKey(authenticatedUserId, postId, request.FileName);
        var uploadedObject = await objectStorageService.UploadAsync(new ObjectStorageUploadRequest
        {
            FileStream = request.FileStream,
            FileSize = request.FileSize,
            ObjectKey = objectKey,
            ContentType = NormalizeContentType(request.ContentType)
        }, cancellationToken);

        Post post;
        try
        {
            post = Post.Create(
                authenticatedUserId,
                uploadedObject.ObjectKey,
                uploadedObject.Url,
                ParsePostType(request.PostType),
                postId);
        }
        catch
        {
            await objectStorageService.DeleteAsync(uploadedObject.ObjectKey, cancellationToken);
            throw;
        }

        var integrationEvent = new PostCreatedIntegrationEvent(
            Guid.NewGuid(),
            post.Id,
            post.UserId,
            post.ObjectKey,
            post.PostUrl,
            post.PostType.ToString().ToUpperInvariant(),
            post.TimestampUtc,
            DateTime.UtcNow);

        var outboxMessage = OutboxMessage.Create(
            nameof(PostCreatedIntegrationEvent),
            nameof(Post),
            post.Id,
            JsonSerializer.Serialize(integrationEvent));

        try
        {
            await unitOfWork.ExecuteTransactionalAsync(async token =>
            {
                await postRepository.AddAsync(post, token);
                await outboxRepository.AddAsync(outboxMessage, token);
            }, cancellationToken);
        }
        catch
        {
            await objectStorageService.DeleteAsync(uploadedObject.ObjectKey, cancellationToken);
            throw;
        }

        await outboxSignal.SignalAsync(outboxMessage.Id, cancellationToken);

        return MapToDto(post);
    }

    public async Task<PostDto?> UpdateAsync(Guid id, Guid authenticatedUserId, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        ValidateFileRequest(request.FileStream, request.FileSize, request.FileName);

        var post = await postRepository.GetByIdForWriteAsync(id, cancellationToken);
        if (post is null)
        {
            return null;
        }

        EnsureOwnership(post, authenticatedUserId);

        var previousObjectKey = post.ObjectKey;
        var newObjectKey = BuildObjectKey(authenticatedUserId, post.Id, request.FileName);
        var uploadedObject = await objectStorageService.UploadAsync(new ObjectStorageUploadRequest
        {
            FileStream = request.FileStream,
            FileSize = request.FileSize,
            ObjectKey = newObjectKey,
            ContentType = NormalizeContentType(request.ContentType)
        }, cancellationToken);

        try
        {
            post.Update(uploadedObject.ObjectKey, uploadedObject.Url, ParsePostType(request.PostType));
            await unitOfWork.ExecuteTransactionalAsync(token => postRepository.UpdateAsync(post, token), cancellationToken);
        }
        catch
        {
            await objectStorageService.DeleteAsync(uploadedObject.ObjectKey, cancellationToken);
            throw;
        }

        if (!string.Equals(previousObjectKey, uploadedObject.ObjectKey, StringComparison.Ordinal))
        {
            await TryDeleteObjectAsync(previousObjectKey, cancellationToken);
        }

        return MapToDto(post);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetByIdForWriteAsync(id, cancellationToken);
        if (post is null)
        {
            return false;
        }

        EnsureOwnership(post, authenticatedUserId);

        await unitOfWork.ExecuteTransactionalAsync(token => postRepository.DeleteAsync(post, token), cancellationToken);
        await TryDeleteObjectAsync(post.ObjectKey, cancellationToken);
        return true;
    }

    private static void EnsureOwnership(Post post, Guid authenticatedUserId)
    {
        if (post.UserId != authenticatedUserId)
        {
            throw new UnauthorizedAccessException("You can only modify your own posts.");
        }
    }

    private static PostType ParsePostType(string? postType)
    {
        if (string.IsNullOrWhiteSpace(postType))
        {
            return PostType.Image;
        }

        if (Enum.TryParse<PostType>(postType, true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException("Post type is invalid.", nameof(postType));
    }

    private static PostDto MapToDto(Post post)
    {
        return new PostDto(
            post.Id,
            post.UserId,
            post.ObjectKey,
            post.PostUrl,
            post.TimestampUtc,
            post.PostType.ToString().ToUpperInvariant());
    }

    private static void ValidateFileRequest(Stream fileStream, long fileSize, string fileName)
    {
        if (fileStream is null)
        {
            throw new ArgumentException("File is required.", nameof(fileStream));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException("File is required.", nameof(fileSize));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }
    }

    private static string BuildObjectKey(Guid userId, Guid postId, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant();
        return $"posts/{userId}/{postId}{safeExtension}";
    }

    private static string NormalizeContentType(string contentType)
    {
        return string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();
    }

    private async Task TryDeleteObjectAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            await objectStorageService.DeleteAsync(objectKey, cancellationToken);
        }
        catch
        {
            // Keep the main workflow successful even if object cleanup needs retry later.
        }
    }
}
