namespace UsersService.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Name,
    string UserName,
    string? ProfilePictureUrl,
    DateTime CreatedAtUtc,
    string Email);
