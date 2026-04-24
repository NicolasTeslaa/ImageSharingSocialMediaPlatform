using UsersService.Application.DTOs;

namespace UsersService.Application.Abstractions;

public interface IUserSearchSyncService
{
    Task UpsertAsync(UserDto user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
