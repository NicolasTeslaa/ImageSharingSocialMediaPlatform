using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using TimelineService.Application;
using TimelineService.Application.Abstractions;
using TimelineService.Infrastructure;
using TimelineService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTimelineApplication()
    .AddTimelineInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

await EnsureDatabaseCreatedWithRetryAsync<TimelineDbContext>(app.Services, app.Logger);

app.MapGet("/health", () => Results.Ok(new { service = "TimelineService", status = "Healthy" }));

app.MapGet("/timeline/{userId:guid}", async (Guid userId, ITimelineQueryService queryService, CancellationToken cancellationToken) =>
{
    var items = await queryService.GetFeedAsync(userId, cancellationToken);
    return Results.Ok(items);
});

app.Run();

static async Task EnsureDatabaseCreatedWithRetryAsync<TDbContext>(IServiceProvider services, ILogger logger, int maxAttempts = 12)
    where TDbContext : DbContext
{
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            return;
        }
        catch (Exception exception) when (IsTransientDatabaseStartupError(exception) && attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Unable to connect to database for {DbContext} on attempt {Attempt}/{MaxAttempts}. Retrying in 5s...",
                typeof(TDbContext).Name,
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    await using var finalScope = services.CreateAsyncScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<TDbContext>();
    await finalDbContext.Database.EnsureCreatedAsync();
}

static bool IsTransientDatabaseStartupError(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
    {
        if (current is SocketException or TimeoutException)
        {
            return true;
        }

        var typeName = current.GetType().Name;
        if (typeName.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("EndOfStream", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var message = current.Message;
        if (message.Contains("Connect Timeout", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("incomplete response", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("connection was aborted", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}
