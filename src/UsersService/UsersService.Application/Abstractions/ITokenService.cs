using UsersService.Application.DTOs;

namespace UsersService.Application.Abstractions;

public interface ITokenService
{
    TokenResponse CreateToken(UserDto user);
}
