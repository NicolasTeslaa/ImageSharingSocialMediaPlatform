namespace UsersService.Application.DTOs;

public sealed record CreateUserRequest(
    string Name,
    string UserName,
    string? ProfilePictureUrl,
    string Email,
    string Password);
