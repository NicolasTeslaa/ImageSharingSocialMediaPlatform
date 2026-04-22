namespace PostsService.Infrastructure.Options;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;
    public string TopicName { get; init; } = "posts.created";
    public int NumPartitions { get; init; } = 3;
    public short ReplicationFactor { get; init; } = 1;
    public int BatchSize { get; init; } = 50;
    public int PollIntervalMilliseconds { get; init; } = 1000;
}
