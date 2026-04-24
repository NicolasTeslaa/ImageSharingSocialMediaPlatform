using UsersService.Application.DTOs;

namespace UsersService.Application.Abstractions;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FollowResultDto> FollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<bool> UnfollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserDto>> GetAllForSearchAsync(CancellationToken cancellationToken = default);
}
