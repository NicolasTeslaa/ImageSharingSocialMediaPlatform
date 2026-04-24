using SearchService.Domain.Entities;

namespace SearchService.Domain.Repositories;

public interface ISearchRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task RebuildAsync(IReadOnlyCollection<SearchUser> users, CancellationToken cancellationToken = default);
    Task UpsertAsync(SearchUser user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SearchUser>> SearchAsync(string term, CancellationToken cancellationToken = default);
}
