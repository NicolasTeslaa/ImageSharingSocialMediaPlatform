using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Domain.Repositories;

namespace PostsService.Application.Services;

public sealed class PostQueryService(IPostRepository postRepository) : IPostQueryService
{
    public async Task<IReadOnlyCollection<PostDto>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        var posts = await postRepository.GetRecentAsync(cancellationToken);

        return posts
            .Select(post => new PostDto(post.Id, post.AuthorUserName, post.Caption, post.CreatedAt))
            .ToArray();
    }
}
