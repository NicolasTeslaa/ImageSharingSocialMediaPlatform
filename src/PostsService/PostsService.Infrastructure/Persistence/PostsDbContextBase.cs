using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;

namespace PostsService.Infrastructure.Persistence;

public abstract class PostsDbContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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

            entity.Property(post => post.ObjectKey)
                .HasMaxLength(500)
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

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");

            entity.HasKey(message => message.Id);

            entity.Property(message => message.Id)
                .ValueGeneratedNever();

            entity.Property(message => message.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(message => message.AggregateType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(message => message.AggregateId)
                .IsRequired();

            entity.Property(message => message.Payload)
                .HasColumnType("longtext")
                .IsRequired();

            entity.Property(message => message.OccurredOnUtc)
                .IsRequired();

            entity.Property(message => message.PublishedOnUtc)
                .IsRequired(false);

            entity.Property(message => message.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(message => message.Attempts)
                .IsRequired();

            entity.Property(message => message.LastError)
                .HasMaxLength(2000)
                .IsRequired(false);

            entity.HasIndex(message => message.Status);
            entity.HasIndex(message => message.AggregateId);
            entity.HasIndex(message => message.OccurredOnUtc);
        });
    }
}
