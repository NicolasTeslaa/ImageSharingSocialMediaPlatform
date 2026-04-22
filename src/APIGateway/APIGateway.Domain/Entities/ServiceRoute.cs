namespace APIGateway.Domain.Entities;

public sealed record ServiceRoute(string ServiceName, string BasePath, string DownstreamUrl);
