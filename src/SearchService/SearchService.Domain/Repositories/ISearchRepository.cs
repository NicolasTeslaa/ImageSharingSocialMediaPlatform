using SearchService.Domain.Entities;

namespace SearchService.Domain.Repositories;

public interface ISearchRepository
{
    Task<IReadOnlyCollection<SearchResult>> SearchAsync(string term, CancellationToken cancellationToken = default);
}
