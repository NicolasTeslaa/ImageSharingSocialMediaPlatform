using PostsService.Application;
using PostsService.Application.Abstractions;
using PostsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostsApplication()
    .AddPostsInfrastructure();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { service = "PostsService", status = "Healthy" }));

app.MapGet("/posts", async (IPostQueryService queryService, CancellationToken cancellationToken) =>
{
    var posts = await queryService.GetRecentAsync(cancellationToken);
    return Results.Ok(posts);
});

app.Run();
