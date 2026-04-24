using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchService.Application.Abstractions;
using SearchService.Domain.Repositories;
using SearchService.Infrastructure.HostedServices;
using SearchService.Infrastructure.Options;
using SearchService.Infrastructure.Repositories;
using SearchService.Infrastructure.Users;

namespace SearchService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ElasticsearchOptions>()
            .Bind(configuration.GetSection(ElasticsearchOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Elasticsearch base url is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.IndexName), "Elasticsearch index name is required.")
            .ValidateOnStart();

        services
            .AddOptions<UsersServiceOptions>()
            .Bind(configuration.GetSection(UsersServiceOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Users service base url is required.")
            .ValidateOnStart();

        services.AddHttpClient<ISearchRepository, ElasticsearchSearchRepository>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<ElasticsearchOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHttpClient<IUsersCatalogService, UsersCatalogService>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersServiceOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHostedService<SearchIndexBootstrapHostedService>();
        return services;
    }
}
