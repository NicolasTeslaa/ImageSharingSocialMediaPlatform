using System.Security.Claims;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UsersService.Application;
using UsersService.Application.Abstractions;
using UsersService.Application.DTOs;
using UsersService.Infrastructure;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("JWT settings were not configured.");

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

await EnsureDatabaseCreatedWithRetryAsync<UsersDbContext>(app.Services, app.Logger);

app.MapGet("/health", () => Results.Ok(new { service = "UsersService", status = "Healthy" }));

app.MapPost("/auth/login", async (
    LoginRequest request,
    IUserService userService,
    ITokenService tokenService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var user = await userService.AuthenticateAsync(request, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var token = tokenService.CreateToken(user);
        return Results.Ok(token);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
});

app.MapGet("/auth/me", async (
    ClaimsPrincipal claimsPrincipal,
    IUserService userService,
    CancellationToken cancellationToken) =>
{
    var userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userIdValue, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await userService.GetByIdAsync(userId, cancellationToken);
    return user is null ? Results.NotFound() : Results.Ok(user);
}).RequireAuthorization();

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

app.MapGet("/users", async (IUserService userService, CancellationToken cancellationToken) =>
{
    var users = await userService.GetAllAsync(cancellationToken);
    return Results.Ok(users);
}).RequireAuthorization();

app.MapGet("/users/{id:guid}", async (Guid id, IUserService userService, CancellationToken cancellationToken) =>
{
    var user = await userService.GetByIdAsync(id, cancellationToken);
    return user is null ? Results.NotFound() : Results.Ok(user);
}).RequireAuthorization();

app.MapGet("/internal/users/search-export", async (IUserService userService, CancellationToken cancellationToken) =>
{
    var users = await userService.GetAllForSearchAsync(cancellationToken);
    return Results.Ok(users);
});

app.MapGet("/users/{id:guid}/following", async (Guid id, IUserService userService, CancellationToken cancellationToken) =>
{
    try
    {
        var followingUserIds = await userService.GetFollowingUserIdsAsync(id, cancellationToken);
        return Results.Ok(followingUserIds);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapPost("/users/{id:guid}/follow", async (
    Guid id,
    ClaimsPrincipal claimsPrincipal,
    IUserService userService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        var follow = await userService.FollowAsync(authenticatedUserId, id, cancellationToken);
        return Results.Ok(follow);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { message = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { message = exception.Message });
    }
}).RequireAuthorization();

app.MapDelete("/users/{id:guid}/follow", async (
    Guid id,
    ClaimsPrincipal claimsPrincipal,
    IUserService userService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        var deleted = await userService.UnfollowAsync(authenticatedUserId, id, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
}).RequireAuthorization();

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
}).RequireAuthorization();

app.MapDelete("/users/{id:guid}", async (Guid id, IUserService userService, CancellationToken cancellationToken) =>
{
    var deleted = await userService.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

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
