namespace TimelineService.Infrastructure.Persistence;

public sealed class ProcessedTimelineEvent
{
    public Guid EventId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int Partition { get; set; }
    public long Offset { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
