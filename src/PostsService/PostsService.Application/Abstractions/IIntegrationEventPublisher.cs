using PostsService.Application.DTOs;

namespace PostsService.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishPostCreatedAsync(PostCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
