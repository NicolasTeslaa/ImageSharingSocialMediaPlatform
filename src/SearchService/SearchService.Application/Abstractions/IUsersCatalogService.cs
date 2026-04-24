using SearchService.Domain.Entities;

namespace SearchService.Application.Abstractions;

public interface IUsersCatalogService
{
    Task<IReadOnlyCollection<SearchUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
