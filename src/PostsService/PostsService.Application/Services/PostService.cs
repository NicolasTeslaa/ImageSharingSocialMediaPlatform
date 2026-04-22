using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Domain.Entities;
using PostsService.Domain.Enums;
using PostsService.Domain.Repositories;

namespace PostsService.Application.Services;

public sealed class PostService(IPostRepository postRepository) : IPostService
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
        var post = Post.Create(
            authenticatedUserId,
            request.PostUrl,
            ParsePostType(request.PostType));

        await postRepository.AddAsync(post, cancellationToken);

        return MapToDto(post);
    }

    public async Task<PostDto?> UpdateAsync(Guid id, Guid authenticatedUserId, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
        {
            return null;
        }

        EnsureOwnership(post, authenticatedUserId);

        post.Update(request.PostUrl, ParsePostType(request.PostType));
        await postRepository.UpdateAsync(post, cancellationToken);

        return MapToDto(post);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
        {
            return false;
        }

        EnsureOwnership(post, authenticatedUserId);

        await postRepository.DeleteAsync(post, cancellationToken);
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
            post.PostUrl,
            post.TimestampUtc,
            post.PostType.ToString().ToUpperInvariant());
    }
}
