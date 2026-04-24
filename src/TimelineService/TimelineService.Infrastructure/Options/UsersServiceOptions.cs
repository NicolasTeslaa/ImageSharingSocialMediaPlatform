namespace TimelineService.Infrastructure.Options;

public sealed class UsersServiceOptions
{
    public const string SectionName = "UsersService";

    public string BaseUrl { get; init; } = string.Empty;
}
