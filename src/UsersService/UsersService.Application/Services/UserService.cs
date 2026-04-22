using UsersService.Application.Abstractions;
using UsersService.Application.DTOs;
using UsersService.Domain.Entities;
using UsersService.Domain.Repositories;

namespace UsersService.Application.Services;

public sealed class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
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

        return MapToDto(user);
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
        return true;
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
