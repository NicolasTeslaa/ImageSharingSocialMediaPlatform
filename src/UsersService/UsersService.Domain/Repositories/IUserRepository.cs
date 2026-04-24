using UsersService.Domain.Entities;

namespace UsersService.Domain.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    Task<UserFollow?> GetFollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddFollowAsync(UserFollow follow, CancellationToken cancellationToken = default);
    Task DeleteFollowAsync(UserFollow follow, CancellationToken cancellationToken = default);
}
