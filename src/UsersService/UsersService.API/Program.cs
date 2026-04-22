using Microsoft.EntityFrameworkCore;
using UsersService.Application;
using UsersService.Application.Abstractions;
using UsersService.Application.DTOs;
using UsersService.Infrastructure;
using UsersService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { service = "UsersService", status = "Healthy" }));

app.MapGet("/users", async (IUserService userService, CancellationToken cancellationToken) =>
{
    var users = await userService.GetAllAsync(cancellationToken);
    return Results.Ok(users);
});

app.MapGet("/users/{id:guid}", async (Guid id, IUserService userService, CancellationToken cancellationToken) =>
{
    var user = await userService.GetByIdAsync(id, cancellationToken);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/users", async (CreateUserRequest request, IUserService userService, CancellationToken cancellationToken) =>
{
    try
    {
        var createdUser = await userService.CreateAsync(request, cancellationToken);
        return Results.Created($"/users/{createdUser.Id}", createdUser);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { message = exception.Message });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { message = "Unable to persist the user due to a database constraint." });
    }
});

app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest request, IUserService userService, CancellationToken cancellationToken) =>
{
    try
    {
        var updatedUser = await userService.UpdateAsync(id, request, cancellationToken);
        return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { message = exception.Message });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { message = "Unable to update the user due to a database constraint." });
    }
});

app.MapDelete("/users/{id:guid}", async (Guid id, IUserService userService, CancellationToken cancellationToken) =>
{
    var deleted = await userService.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
