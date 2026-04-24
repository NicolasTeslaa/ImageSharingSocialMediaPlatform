using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimelineService.Domain.Entities;
using TimelineService.Domain.Repositories;
using TimelineService.Infrastructure.Options;
using TimelineService.Infrastructure.Persistence;

namespace TimelineService.Infrastructure.Messaging;

public sealed class TimelineProjectionConsumerBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ITimelineRepository timelineRepository,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<TimelineProjectionConsumerBackgroundService> logger) : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ResetProjectionAsync(stoppingToken);

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetPartitionsAssignedHandler((_, partitions) =>
                partitions.Select(partition => new TopicPartitionOffset(partition, Offset.Beginning)))
            .Build();

        consumer.Subscribe(_kafkaOptions.TopicName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                if (consumeResult?.Message?.Value is null)
                {
                    continue;
                }

                await ProcessMessageAsync(consumeResult, stoppingToken);

                if (_kafkaOptions.ConsumeDelayMilliseconds > 0)
                {
                    await Task.Delay(_kafkaOptions.ConsumeDelayMilliseconds, stoppingToken);
                }
            }
            catch (ConsumeException exception)
            {
                logger.LogError(exception, "Kafka consume error while building timeline projection.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while processing timeline projection.");
            }
        }

        consumer.Close();
    }

    private async Task ResetProjectionAsync(CancellationToken cancellationToken)
    {
        await timelineRepository.ClearAsync(cancellationToken);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TimelineDbContext>();

        await dbContext.ProcessedTimelineEvents.ExecuteDeleteAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<PostCreatedIntegrationEvent>(consumeResult.Message.Value);
        if (integrationEvent is null)
        {
            throw new InvalidOperationException("Timeline message payload could not be deserialized.");
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TimelineDbContext>();

        var alreadyProcessed = await dbContext.ProcessedTimelineEvents
            .AnyAsync(item => item.EventId == integrationEvent.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        await timelineRepository.AddAsync(new TimelineItem(
            integrationEvent.PostId,
            integrationEvent.UserId,
            integrationEvent.PostUrl,
            DateTime.SpecifyKind(integrationEvent.TimestampUtc, DateTimeKind.Utc)), cancellationToken);

        dbContext.ProcessedTimelineEvents.Add(new ProcessedTimelineEvent
        {
            EventId = integrationEvent.EventId,
            PostId = integrationEvent.PostId,
            UserId = integrationEvent.UserId,
            Topic = consumeResult.Topic,
            Partition = consumeResult.Partition.Value,
            Offset = consumeResult.Offset.Value,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
