using SearchService.Application.Abstractions;
using SearchService.Application.DTOs;
using SearchService.Domain.Entities;
using SearchService.Domain.Repositories;

namespace SearchService.Application.Services;

public sealed class SearchIndexService(
    ISearchRepository searchRepository,
    IUsersCatalogService usersCatalogService) : ISearchIndexService
{
    public async Task UpsertAsync(SearchUserUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        await searchRepository.UpsertAsync(new SearchUser(
            request.Id,
            request.Name.Trim(),
            request.UserName.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(request.ProfilePictureUrl) ? null : request.ProfilePictureUrl.Trim(),
            DateTime.SpecifyKind(request.CreatedAtUtc, DateTimeKind.Utc)), cancellationToken);
    }

    public Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return searchRepository.DeleteAsync(userId, cancellationToken);
    }

    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        var users = await usersCatalogService.GetAllUsersAsync(cancellationToken);
        await searchRepository.RebuildAsync(users, cancellationToken);
    }

    private static void Validate(SearchUserUpsertRequest request)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(request.Id));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ArgumentException("User name is required.", nameof(request.UserName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }
    }
}
