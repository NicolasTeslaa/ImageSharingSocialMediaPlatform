using TimelineService.Application;
using TimelineService.Application.Abstractions;
using TimelineService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTimelineApplication()
    .AddTimelineInfrastructure();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { service = "TimelineService", status = "Healthy" }));

app.MapGet("/timeline/{userName}", async (string userName, ITimelineQueryService queryService, CancellationToken cancellationToken) =>
{
    var items = await queryService.GetByUserAsync(userName, cancellationToken);
    return Results.Ok(items);
});

app.Run();
