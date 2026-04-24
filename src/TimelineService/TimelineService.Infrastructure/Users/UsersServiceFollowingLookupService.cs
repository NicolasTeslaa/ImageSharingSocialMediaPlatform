using System.Net.Http.Json;
using TimelineService.Application.Abstractions;

namespace TimelineService.Infrastructure.Users;

public sealed class UsersServiceFollowingLookupService(HttpClient httpClient) : IFollowingLookupService
{
    public async Task<IReadOnlyCollection<Guid>> GetFollowingUserIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/users/{userId}/following", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();

        var userIds = await response.Content.ReadFromJsonAsync<Guid[]>(cancellationToken: cancellationToken);
        return userIds ?? [];
    }
}
