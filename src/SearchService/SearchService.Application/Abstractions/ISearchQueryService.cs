using SearchService.Application.DTOs;

namespace SearchService.Application.Abstractions;

public interface ISearchQueryService
{
    Task<IReadOnlyCollection<SearchResultDto>> SearchAsync(string term, CancellationToken cancellationToken = default);
}
