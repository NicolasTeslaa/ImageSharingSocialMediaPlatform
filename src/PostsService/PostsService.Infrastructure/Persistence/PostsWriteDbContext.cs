using Microsoft.EntityFrameworkCore;

namespace PostsService.Infrastructure.Persistence;

public sealed class PostsWriteDbContext(DbContextOptions<PostsWriteDbContext> options)
    : PostsDbContextBase(options);
