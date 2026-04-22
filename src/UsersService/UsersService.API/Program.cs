using System.Security.Claims;
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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

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
