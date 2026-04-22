using APIGateway.Application;
using APIGateway.Application.Abstractions;
using APIGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGatewayApplication()
    .AddGatewayInfrastructure();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { service = "APIGateway", status = "Healthy" }));

app.MapGet("/gateway/routes", async (IRouteQueryService queryService, CancellationToken cancellationToken) =>
{
    var routes = await queryService.GetRoutesAsync(cancellationToken);
    return Results.Ok(routes);
});

app.Run();
