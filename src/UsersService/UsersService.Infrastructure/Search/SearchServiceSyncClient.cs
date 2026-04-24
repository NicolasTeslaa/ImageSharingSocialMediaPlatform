using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using UsersService.Application.Abstractions;
using UsersService.Application.DTOs;

namespace UsersService.Infrastructure.Search;

public sealed class SearchServiceSyncClient(
    HttpClient httpClient,
    ILogger<SearchServiceSyncClient> logger) : IUserSearchSyncService
{
    public async Task UpsertAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        var payload = new SearchUserSyncDto(
            user.Id,
            user.Name,
            user.UserName,
            user.Email,
            user.ProfilePictureUrl,
            user.CreatedAtUtc);

        try
        {
            var response = await httpClient.PutAsJsonAsync($"/search/users/{user.Id}", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to sync user {UserId} to SearchService.", user.Id);
        }
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/search/users/{userId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete user {UserId} from SearchService index.", userId);
        }
    }
}
