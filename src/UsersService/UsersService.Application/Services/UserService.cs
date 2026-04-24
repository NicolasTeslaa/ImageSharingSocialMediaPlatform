using UsersService.Application.Abstractions;
using UsersService.Application.DTOs;
using UsersService.Domain.Entities;
using UsersService.Domain.Repositories;

namespace UsersService.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserSearchSyncService userSearchSyncService) : IUserService
{
    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToArray();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        await EnsureUniqueAsync(request.UserName, request.Email, null, cancellationToken);

        var user = User.Create(
            request.Name,
            request.UserName,
            request.Email,
            passwordHasher.Hash(request.Password),
            request.ProfilePictureUrl);

        await userRepository.AddAsync(user, cancellationToken);
        await userSearchSyncService.UpsertAsync(MapToDto(user), cancellationToken);

        return MapToDto(user);
    }

    public async Task<UserDto?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return passwordHasher.Verify(request.Password, user.PasswordHash)
            ? MapToDto(user)
            : null;
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        await EnsureUniqueAsync(request.UserName, request.Email, id, cancellationToken);

        user.Update(request.Name, request.UserName, request.Email, request.ProfilePictureUrl);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.UpdatePassword(passwordHasher.Hash(request.Password));
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await userSearchSyncService.UpsertAsync(MapToDto(user), cancellationToken);

        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await userRepository.DeleteAsync(user, cancellationToken);
        await userSearchSyncService.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<FollowResultDto> FollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        if (followerUserId == Guid.Empty)
        {
            throw new ArgumentException("Follower user id is required.", nameof(followerUserId));
        }

        if (followedUserId == Guid.Empty)
        {
            throw new ArgumentException("Followed user id is required.", nameof(followedUserId));
        }

        if (followerUserId == followedUserId)
        {
            throw new InvalidOperationException("A user cannot follow themselves.");
        }

        await EnsureUserExistsAsync(followerUserId, cancellationToken);
        await EnsureUserExistsAsync(followedUserId, cancellationToken);

        var existingFollow = await userRepository.GetFollowAsync(followerUserId, followedUserId, cancellationToken);
        if (existingFollow is not null)
        {
            return new FollowResultDto(existingFollow.FollowerUserId, existingFollow.FollowedUserId, existingFollow.CreatedAtUtc);
        }

        var follow = UserFollow.Create(followerUserId, followedUserId);
        await userRepository.AddFollowAsync(follow, cancellationToken);

        return new FollowResultDto(follow.FollowerUserId, follow.FollowedUserId, follow.CreatedAtUtc);
    }

    public async Task<bool> UnfollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        if (followerUserId == Guid.Empty)
        {
            throw new ArgumentException("Follower user id is required.", nameof(followerUserId));
        }

        if (followedUserId == Guid.Empty)
        {
            throw new ArgumentException("Followed user id is required.", nameof(followedUserId));
        }

        var follow = await userRepository.GetFollowAsync(followerUserId, followedUserId, cancellationToken);
        if (follow is null)
        {
            return false;
        }

        await userRepository.DeleteFollowAsync(follow, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        await EnsureUserExistsAsync(userId, cancellationToken);
        return await userRepository.GetFollowingUserIdsAsync(userId, cancellationToken);
    }

    public Task<IReadOnlyCollection<UserDto>> GetAllForSearchAsync(CancellationToken cancellationToken = default)
    {
        return GetAllAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(
        string userName,
        string email,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        var existingByUserName = await userRepository.GetByUserNameAsync(userName, cancellationToken);
        if (existingByUserName is not null && existingByUserName.Id != currentUserId)
        {
            throw new InvalidOperationException("Username is already in use.");
        }

        var existingByEmail = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != currentUserId)
        {
            throw new InvalidOperationException("Email is already in use.");
        }
    }

    private async Task EnsureUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (existingUser is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }
    }

    private static void ValidateCreateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }
    }

    private static void ValidateUpdateRequest(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ArgumentException("Username is required.", nameof(request.UserName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.UserName,
            user.ProfilePictureUrl,
            user.CreatedAtUtc,
            user.Email);
    }
}
