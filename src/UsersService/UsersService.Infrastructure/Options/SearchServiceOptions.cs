namespace UsersService.Infrastructure.Options;

public sealed class SearchServiceOptions
{
    public const string SectionName = "SearchService";

    public string BaseUrl { get; init; } = "http://localhost:5239";
}
