namespace UsersService.Application.DTOs;

public sealed record TokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string TokenType,
    UserDto User);
