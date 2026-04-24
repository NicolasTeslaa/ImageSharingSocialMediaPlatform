using SearchService.Application;
using SearchService.Application.Abstractions;
using SearchService.Application.DTOs;
using SearchService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSearchApplication()
    .AddSearchInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { service = "SearchService", status = "Healthy" }));

app.MapGet("/search", async (string? term, ISearchQueryService queryService, CancellationToken cancellationToken) =>
{
    var results = await queryService.SearchAsync(term ?? string.Empty, cancellationToken);
    return Results.Ok(results);
});

app.MapGet("/search/users", async (string? q, ISearchQueryService queryService, CancellationToken cancellationToken) =>
{
    var results = await queryService.SearchAsync(q ?? string.Empty, cancellationToken);
    return Results.Ok(results);
});

app.MapPost("/search/users", async (
    SearchUserUpsertRequest request,
    ISearchIndexService indexService,
    CancellationToken cancellationToken) =>
{
    await indexService.UpsertAsync(request, cancellationToken);
    return Results.Accepted();
});

app.MapPut("/search/users/{id:guid}", async (
    Guid id,
    SearchUserUpsertRequest request,
    ISearchIndexService indexService,
    CancellationToken cancellationToken) =>
{
    if (id != request.Id)
    {
        return Results.BadRequest(new { message = "Route id must match request id." });
    }

    await indexService.UpsertAsync(request, cancellationToken);
    return Results.Accepted();
});

app.MapDelete("/search/users/{id:guid}", async (
    Guid id,
    ISearchIndexService indexService,
    CancellationToken cancellationToken) =>
{
    await indexService.DeleteAsync(id, cancellationToken);
    return Results.Accepted();
});

app.MapPost("/search/rebuild", async (
    ISearchIndexService indexService,
    CancellationToken cancellationToken) =>
{
    await indexService.RebuildAsync(cancellationToken);
    return Results.Accepted();
});

app.Run();
