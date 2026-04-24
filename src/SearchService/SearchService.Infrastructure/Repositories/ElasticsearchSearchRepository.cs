using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SearchService.Domain.Entities;
using SearchService.Domain.Repositories;
using SearchService.Infrastructure.Options;

namespace SearchService.Infrastructure.Repositories;

public sealed class ElasticsearchSearchRepository(
    HttpClient httpClient,
    Microsoft.Extensions.Options.IOptions<ElasticsearchOptions> options) : ISearchRepository
{
    private readonly ElasticsearchOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var indexExistsResponse = await httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Head, $"/{_options.IndexName}"),
            cancellationToken);

        if (indexExistsResponse.IsSuccessStatusCode)
        {
            return;
        }

        var payload = """
        {
          "settings": {
            "analysis": {
              "normalizer": {
                "lowercase_normalizer": {
                  "type": "custom",
                  "char_filter": [],
                  "filter": ["lowercase"]
                }
              }
            }
          },
          "mappings": {
            "properties": {
              "id": { "type": "keyword" },
              "name": {
                "type": "text",
                "fields": {
                  "raw": { "type": "keyword", "normalizer": "lowercase_normalizer" }
                }
              },
              "userName": {
                "type": "text",
                "fields": {
                  "raw": { "type": "keyword", "normalizer": "lowercase_normalizer" }
                }
              },
              "email": {
                "type": "text",
                "fields": {
                  "raw": { "type": "keyword", "normalizer": "lowercase_normalizer" }
                }
              },
              "profilePictureUrl": { "type": "keyword", "index": false },
              "createdAtUtc": { "type": "date" }
            }
          }
        }
        """;

        var createResponse = await httpClient.PutAsync(
            $"/{_options.IndexName}",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            cancellationToken);

        if (createResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            return;
        }

        createResponse.EnsureSuccessStatusCode();
    }

    public async Task RebuildAsync(IReadOnlyCollection<SearchUser> users, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var deleteResponse = await httpClient.DeleteAsync($"/{_options.IndexName}", cancellationToken);
        if (deleteResponse.StatusCode != HttpStatusCode.NotFound)
        {
            deleteResponse.EnsureSuccessStatusCode();
        }

        await InitializeAsync(cancellationToken);

        foreach (var user in users)
        {
            await UpsertAsync(user, cancellationToken);
        }

        await RefreshAsync(cancellationToken);
    }

    public async Task UpsertAsync(SearchUser user, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            id = user.Id,
            name = user.Name,
            userName = user.UserName,
            email = user.Email,
            profilePictureUrl = user.ProfilePictureUrl,
            createdAtUtc = user.CreatedAtUtc
        }, JsonOptions);

        var response = await httpClient.PutAsync(
            $"/{_options.IndexName}/_doc/{user.Id}",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        await RefreshAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var response = await httpClient.DeleteAsync($"/{_options.IndexName}/_doc/{userId}", cancellationToken);
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }

        await RefreshAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SearchUser>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var trimmedTerm = term.Trim();
        string payload;

        if (string.IsNullOrWhiteSpace(trimmedTerm))
        {
            payload = """
            {
              "size": 20,
              "sort": [
                { "createdAtUtc": { "order": "desc" } }
              ],
              "query": {
                "match_all": {}
              }
            }
            """;
        }
        else
        {
            var loweredTerm = trimmedTerm.ToLowerInvariant();
            payload = JsonSerializer.Serialize(new
            {
                size = 20,
                query = new
                {
                    @bool = new
                    {
                        should = new object[]
                        {
                            new
                            {
                                multi_match = new
                                {
                                    query = trimmedTerm,
                                    fields = new[] { "name^4", "userName^5", "email^3" },
                                    fuzziness = "AUTO"
                                }
                            },
                            new
                            {
                                wildcard = new Dictionary<string, object>
                                {
                                    ["userName.raw"] = new { value = $"*{loweredTerm}*", boost = 6 }
                                }
                            },
                            new
                            {
                                wildcard = new Dictionary<string, object>
                                {
                                    ["email.raw"] = new { value = $"*{loweredTerm}*", boost = 5 }
                                }
                            },
                            new
                            {
                                wildcard = new Dictionary<string, object>
                                {
                                    ["name.raw"] = new { value = $"*{loweredTerm}*", boost = 4 }
                                }
                            }
                        },
                        minimum_should_match = 1
                    }
                }
            }, JsonOptions);
        }

        var response = await httpClient.PostAsync(
            $"/{_options.IndexName}/_search",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var searchResponse = await response.Content.ReadFromJsonAsync<ElasticsearchSearchResponse>(JsonOptions, cancellationToken)
            ?? new ElasticsearchSearchResponse();

        return searchResponse.Hits.HitItems
            .Select(hit => hit.Source)
            .Where(source => source is not null)
            .Select(source => new SearchUser(
                source!.Id,
                source.Name,
                source.UserName,
                source.Email,
                source.ProfilePictureUrl,
                DateTime.SpecifyKind(source.CreatedAtUtc, DateTimeKind.Utc)))
            .ToArray();
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync($"/{_options.IndexName}/_refresh", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed class ElasticsearchSearchResponse
    {
        [JsonPropertyName("hits")]
        public ElasticsearchHits Hits { get; set; } = new();
    }

    private sealed class ElasticsearchHits
    {
        [JsonPropertyName("hits")]
        public List<ElasticsearchHit> HitItems { get; set; } = [];
    }

    private sealed class ElasticsearchHit
    {
        [JsonPropertyName("_source")]
        public ElasticsearchUserDocument? Source { get; set; }
    }

    private sealed class ElasticsearchUserDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
