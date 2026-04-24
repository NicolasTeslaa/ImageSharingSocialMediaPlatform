namespace UsersService.Application.DTOs;

public sealed record SearchUserSyncDto(
    Guid Id,
    string Name,
    string UserName,
    string Email,
    string? ProfilePictureUrl,
    DateTime CreatedAtUtc);
