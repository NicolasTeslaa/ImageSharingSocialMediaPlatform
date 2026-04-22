namespace APIGateway.Application.DTOs;

public sealed record ServiceRouteDto(string ServiceName, string BasePath, string DownstreamUrl);
