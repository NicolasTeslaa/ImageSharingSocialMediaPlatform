using SearchService.Application;
using SearchService.Application.Abstractions;
using SearchService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSearchApplication()
    .AddSearchInfrastructure();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { service = "SearchService", status = "Healthy" }));

app.MapGet("/search", async (string? term, ISearchQueryService queryService, CancellationToken cancellationToken) =>
{
    var results = await queryService.SearchAsync(term ?? string.Empty, cancellationToken);
    return Results.Ok(results);
});

app.Run();
