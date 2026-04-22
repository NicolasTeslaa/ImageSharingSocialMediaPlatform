using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostsService.Infrastructure.Options;

namespace PostsService.Infrastructure.Messaging;

public sealed class KafkaTopicInitializerHostedService(
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<KafkaTopicInitializerHostedService> logger) : IHostedService
{
    private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers
        }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = _kafkaOptions.TopicName,
                    NumPartitions = _kafkaOptions.NumPartitions,
                    ReplicationFactor = _kafkaOptions.ReplicationFactor
                }
            ]);
        }
        catch (CreateTopicsException exception) when (
            exception.Results.All(result => result.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Kafka topic {TopicName} already exists.", _kafkaOptions.TopicName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
