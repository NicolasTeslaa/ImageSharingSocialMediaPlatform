using SearchService.Application.DTOs;

namespace SearchService.Application.Abstractions;

public interface ISearchIndexService
{
    Task UpsertAsync(SearchUserUpsertRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RebuildAsync(CancellationToken cancellationToken = default);
}
