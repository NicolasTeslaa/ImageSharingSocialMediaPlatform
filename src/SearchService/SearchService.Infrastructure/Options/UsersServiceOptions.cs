namespace SearchService.Infrastructure.Options;

public sealed class UsersServiceOptions
{
    public const string SectionName = "UsersService";

    public string BaseUrl { get; init; } = "http://localhost:5166";
}
