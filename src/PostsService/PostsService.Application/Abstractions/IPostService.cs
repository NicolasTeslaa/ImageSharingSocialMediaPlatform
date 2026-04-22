using PostsService.Application.DTOs;

namespace PostsService.Application.Abstractions;

public interface IPostService
{
    Task<IReadOnlyCollection<PostDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PostDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PostDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto> CreateAsync(Guid authenticatedUserId, CreatePostRequest request, CancellationToken cancellationToken = default);
    Task<PostDto?> UpdateAsync(Guid id, Guid authenticatedUserId, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
