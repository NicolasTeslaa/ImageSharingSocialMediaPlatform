using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Infrastructure.Options;
using System.Text.Json;

namespace PostsService.Infrastructure.Messaging;

public sealed class KafkaPostCreatedPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly IProducer<string, string> _producer;

    public KafkaPostCreatedPublisher(IOptions<KafkaOptions> kafkaOptions)
    {
        _kafkaOptions = kafkaOptions.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = GetBootstrapServers(),
            EnableIdempotence = true,
            Acks = Acks.All,
            LingerMs = 5,
            CompressionType = CompressionType.Lz4
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishPostCreatedAsync(PostCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = new Message<string, string>
        {
            Key = integrationEvent.PostId.ToString(),
            Value = JsonSerializer.Serialize(integrationEvent),
            Headers =
            [
                new Header("event-id", System.Text.Encoding.UTF8.GetBytes(integrationEvent.EventId.ToString())),
                new Header("event-type", System.Text.Encoding.UTF8.GetBytes(nameof(PostCreatedIntegrationEvent)))
            ]
        };

        await _producer.ProduceAsync(_kafkaOptions.TopicName, message, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }

    private string GetBootstrapServers()
    {
        if (string.IsNullOrWhiteSpace(_kafkaOptions.BootstrapServers))
        {
            throw new InvalidOperationException("Kafka bootstrap servers were not configured.");
        }

        return _kafkaOptions.BootstrapServers;
    }
}
