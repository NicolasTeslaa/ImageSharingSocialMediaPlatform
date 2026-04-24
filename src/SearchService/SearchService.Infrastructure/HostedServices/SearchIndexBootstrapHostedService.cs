using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchService.Application.Abstractions;
using SearchService.Domain.Repositories;

namespace SearchService.Infrastructure.HostedServices;

public sealed class SearchIndexBootstrapHostedService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SearchIndexBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISearchRepository>();
            var indexService = scope.ServiceProvider.GetRequiredService<ISearchIndexService>();

            await repository.InitializeAsync(cancellationToken);
            await indexService.RebuildAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to bootstrap Elasticsearch index for SearchService.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
