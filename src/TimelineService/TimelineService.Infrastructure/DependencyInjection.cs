using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimelineService.Application.Abstractions;
using TimelineService.Domain.Repositories;
using TimelineService.Infrastructure.Messaging;
using TimelineService.Infrastructure.Options;
using TimelineService.Infrastructure.Persistence;
using TimelineService.Infrastructure.Repositories;
using TimelineService.Infrastructure.Users;

namespace TimelineService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTimelineInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TimelineDatabase")
            ?? throw new InvalidOperationException("Connection string 'TimelineDatabase' was not found.");

        services
            .AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka bootstrap servers are required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TopicName), "Kafka topic name is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConsumerGroupId), "Kafka consumer group id is required.")
            .ValidateOnStart();

        services.AddDbContext<TimelineDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

        services
            .AddOptions<UsersServiceOptions>()
            .Bind(configuration.GetSection(UsersServiceOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Users service base url is required.")
            .ValidateOnStart();

        services.AddHttpClient<IFollowingLookupService, UsersServiceFollowingLookupService>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersServiceOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddSingleton<ITimelineRepository, InMemoryTimelineRepository>();
        services.AddHostedService<TimelineProjectionConsumerBackgroundService>();
        return services;
    }
}
