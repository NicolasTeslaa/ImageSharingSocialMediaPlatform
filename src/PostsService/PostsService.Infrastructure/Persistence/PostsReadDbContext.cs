using Microsoft.EntityFrameworkCore;

namespace PostsService.Infrastructure.Persistence;

public sealed class PostsReadDbContext(DbContextOptions<PostsReadDbContext> options)
    : PostsDbContextBase(options);
