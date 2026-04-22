namespace UsersService.Application.DTOs;

public sealed record UpdateUserRequest(
    string Name,
    string UserName,
    string? ProfilePictureUrl,
    string Email,
    string? Password);
