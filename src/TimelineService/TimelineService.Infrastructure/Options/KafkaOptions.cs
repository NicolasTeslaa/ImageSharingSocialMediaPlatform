namespace TimelineService.Infrastructure.Options;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;
    public string TopicName { get; init; } = "posts.created";
    public string ConsumerGroupId { get; init; } = "timeline-service";
    public int ConsumeDelayMilliseconds { get; init; } = 250;
}
