using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;

namespace PostsService.Infrastructure.Persistence;

public sealed class PostsDbContext(DbContextOptions<PostsDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");

            entity.HasKey(post => post.Id);

            entity.Property(post => post.Id)
                .ValueGeneratedNever();

            entity.Property(post => post.UserId)
                .IsRequired();

            entity.Property(post => post.PostUrl)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(post => post.TimestampUtc)
                .IsRequired();

            entity.Property(post => post.PostType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(post => post.UserId);
            entity.HasIndex(post => post.TimestampUtc);
        });
    }
}
