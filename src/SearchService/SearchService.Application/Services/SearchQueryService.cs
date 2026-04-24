using SearchService.Application.Abstractions;
using SearchService.Application.DTOs;
using SearchService.Domain.Repositories;

namespace SearchService.Application.Services;

public sealed class SearchQueryService(ISearchRepository searchRepository) : ISearchQueryService
{
    public async Task<IReadOnlyCollection<SearchResultDto>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var results = await searchRepository.SearchAsync(term, cancellationToken);

        return results
            .Select(result => new SearchResultDto(
                result.Id,
                result.Name,
                result.UserName,
                result.Email,
                result.ProfilePictureUrl,
                result.CreatedAtUtc))
            .ToArray();
    }
}
