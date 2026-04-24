using System.Net.Http.Json;
using SearchService.Application.Abstractions;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Users;

public sealed class UsersCatalogService(HttpClient httpClient) : IUsersCatalogService
{
    public async Task<IReadOnlyCollection<SearchUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await httpClient.GetFromJsonAsync<UserCatalogItem[]>("/internal/users/search-export", cancellationToken)
            ?? [];

        return users
            .Select(user => new SearchUser(
                user.Id,
                user.Name,
                user.UserName,
                user.Email,
                user.ProfilePictureUrl,
                DateTime.SpecifyKind(user.CreatedAtUtc, DateTimeKind.Utc)))
            .ToArray();
    }

    private sealed record UserCatalogItem(
        Guid Id,
        string Name,
        string UserName,
        string? ProfilePictureUrl,
        DateTime CreatedAtUtc,
        string Email);
}
