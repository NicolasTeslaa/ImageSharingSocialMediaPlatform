using Microsoft.EntityFrameworkCore;

namespace TimelineService.Infrastructure.Persistence;

public sealed class TimelineDbContext(DbContextOptions<TimelineDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedTimelineEvent> ProcessedTimelineEvents => Set<ProcessedTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedTimelineEvent>(entity =>
        {
            entity.ToTable("timeline_processed_events");

            entity.HasKey(item => item.EventId);

            entity.Property(item => item.EventId)
                .ValueGeneratedNever();

            entity.Property(item => item.PostId)
                .IsRequired();

            entity.Property(item => item.UserId)
                .IsRequired();

            entity.Property(item => item.Topic)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.Partition)
                .IsRequired();

            entity.Property(item => item.Offset)
                .IsRequired();

            entity.Property(item => item.ProcessedAtUtc)
                .IsRequired();

            entity.HasIndex(item => item.PostId);
            entity.HasIndex(item => item.UserId);
            entity.HasIndex(item => new { item.Topic, item.Partition, item.Offset });
        });
    }
}
