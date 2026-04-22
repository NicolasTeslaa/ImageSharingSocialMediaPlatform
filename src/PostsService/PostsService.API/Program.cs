using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using PostsService.Application;
using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Infrastructure;
using PostsService.Infrastructure.Persistence;
using PostsService.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("JWT settings were not configured.");

builder.Services
    .AddPostsApplication()
    .AddPostsInfrastructure(builder.Configuration);

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
    var dbContext = scope.ServiceProvider.GetRequiredService<PostsWriteDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { service = "PostsService", status = "Healthy" }));

app.MapGet("/posts", async (IPostService postService, CancellationToken cancellationToken) =>
{
    var posts = await postService.GetAllAsync(cancellationToken);
    return Results.Ok(posts);
}).RequireAuthorization();

app.MapGet("/posts/{id:guid}", async (Guid id, IPostService postService, CancellationToken cancellationToken) =>
{
    var post = await postService.GetByIdAsync(id, cancellationToken);
    return post is null ? Results.NotFound() : Results.Ok(post);
}).RequireAuthorization();

app.MapGet("/users/{userId:guid}/posts", async (Guid userId, IPostService postService, CancellationToken cancellationToken) =>
{
    var posts = await postService.GetByUserIdAsync(userId, cancellationToken);
    return Results.Ok(posts);
}).RequireAuthorization();

app.MapPost("/posts", async (
    ClaimsPrincipal claimsPrincipal,
    IFormFile file,
    [FromForm] string? postType,
    IPostService postService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!TryGetAuthenticatedUserId(claimsPrincipal, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        await using var stream = file.OpenReadStream();
        var post = await postService.CreateAsync(authenticatedUserId, new CreatePostRequest
        {
            FileStream = stream,
            FileSize = file.Length,
            FileName = file.FileName,
            ContentType = file.ContentType,
            PostType = postType
        }, cancellationToken);
        return Results.Created($"/posts/{post.Id}", post);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { message = "Unable to persist the post due to a database constraint." });
    }
}).RequireAuthorization();

app.MapPut("/posts/{id:guid}", async (
    Guid id,
    ClaimsPrincipal claimsPrincipal,
    IFormFile file,
    [FromForm] string? postType,
    IPostService postService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!TryGetAuthenticatedUserId(claimsPrincipal, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        await using var stream = file.OpenReadStream();
        var post = await postService.UpdateAsync(id, authenticatedUserId, new UpdatePostRequest
        {
            FileStream = stream,
            FileSize = file.Length,
            FileName = file.FileName,
            ContentType = file.ContentType,
            PostType = postType
        }, cancellationToken);
        return post is null ? Results.NotFound() : Results.Ok(post);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { message = exception.Message });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { message = "Unable to update the post due to a database constraint." });
    }
}).RequireAuthorization();

app.MapDelete("/posts/{id:guid}", async (
    Guid id,
    ClaimsPrincipal claimsPrincipal,
    IPostService postService,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!TryGetAuthenticatedUserId(claimsPrincipal, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        var deleted = await postService.DeleteAsync(id, authenticatedUserId, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
}).RequireAuthorization();

app.Run();

static bool TryGetAuthenticatedUserId(ClaimsPrincipal claimsPrincipal, out Guid authenticatedUserId)
{
    var userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(userIdValue, out authenticatedUserId);
}
