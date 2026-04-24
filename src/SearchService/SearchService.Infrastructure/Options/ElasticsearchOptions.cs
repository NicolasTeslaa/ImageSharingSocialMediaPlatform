namespace SearchService.Infrastructure.Options;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string BaseUrl { get; init; } = "http://localhost:9200";
    public string IndexName { get; init; } = "users";
}
