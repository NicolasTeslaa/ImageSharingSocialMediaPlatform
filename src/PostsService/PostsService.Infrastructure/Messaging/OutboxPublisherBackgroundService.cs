using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostsService.Application.Abstractions;
using PostsService.Application.DTOs;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Options;

namespace PostsService.Infrastructure.Messaging;

public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<OutboxPublisherBackgroundService> logger,
    InMemoryOutboxSignal outboxSignal) : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;
    private readonly SemaphoreSlim _publishSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_kafkaOptions.PollIntervalMilliseconds));

        var signalTask = ConsumeSignalsAsync(stoppingToken);
        var pollingTask = PollAsync(timer, stoppingToken);

        await Task.WhenAll(signalTask, pollingTask);
    }

    private async Task ConsumeSignalsAsync(CancellationToken cancellationToken)
    {
        await foreach (var _ in outboxSignal.ReadAllAsync(cancellationToken))
        {
            await PublishPendingAsync(cancellationToken);
        }
    }

    private async Task PollAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await PublishPendingAsync(cancellationToken);
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        if (!await _publishSemaphore.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var integrationEventPublisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IPostsUnitOfWork>();

            var messages = await outboxRepository.GetPendingBatchAsync(_kafkaOptions.BatchSize, cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var integrationEvent = JsonSerializer.Deserialize<PostCreatedIntegrationEvent>(message.Payload)
                        ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");

                    await integrationEventPublisher.PublishPostCreatedAsync(integrationEvent, cancellationToken);

                    await unitOfWork.ExecuteTransactionalAsync(async token =>
                    {
                        var trackedMessage = await outboxRepository.GetByIdAsync(message.Id, token)
                            ?? throw new InvalidOperationException($"Outbox message {message.Id} was not found.");

                        trackedMessage.MarkPublished();
                    }, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to publish outbox message {OutboxMessageId}", message.Id);

                    await unitOfWork.ExecuteTransactionalAsync(async token =>
                    {
                        var trackedMessage = await outboxRepository.GetByIdAsync(message.Id, token)
                            ?? throw new InvalidOperationException($"Outbox message {message.Id} was not found.");

                        trackedMessage.MarkFailed(exception.Message);
                        trackedMessage.ResetForRetry();
                    }, cancellationToken);
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while publishing pending outbox messages.");
        }
        finally
        {
            _publishSemaphore.Release();
        }
    }
}
