using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using PostsService.Application.Abstractions;
using PostsService.Domain.Repositories;
using PostsService.Infrastructure.Messaging;
using PostsService.Infrastructure.Options;
using PostsService.Infrastructure.Persistence;
using PostsService.Infrastructure.Repositories;
using PostsService.Infrastructure.Security;
using PostsService.Infrastructure.Storage;

namespace PostsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var writeConnectionString = configuration.GetConnectionString("PostsWriteDatabase")
            ?? throw new InvalidOperationException("Connection string 'PostsWriteDatabase' was not found.");

        var readConnectionString = configuration.GetConnectionString("PostsReadDatabase")
            ?? throw new InvalidOperationException("Connection string 'PostsReadDatabase' was not found.");

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "JWT secret key is required.")
            .Validate(options => options.SecretKey.Length >= 32, "JWT secret key must be at least 32 characters.")
            .ValidateOnStart();

        services
            .AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka bootstrap servers are required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TopicName), "Kafka topic name is required.")
            .ValidateOnStart();

        services
            .AddOptions<ObjectStorageOptions>()
            .Bind(configuration.GetSection(ObjectStorageOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "Object storage endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PublicEndpoint), "Object storage public endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "Object storage access key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Object storage secret key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BucketName), "Object storage bucket name is required.")
            .ValidateOnStart();

        services.AddDbContext<PostsWriteDbContext>(options =>
            options.UseMySql(
                writeConnectionString,
                ServerVersion.AutoDetect(writeConnectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

        services.AddDbContext<PostsReadDbContext>(options =>
            options.UseMySql(
                readConnectionString,
                ServerVersion.AutoDetect(readConnectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPostsUnitOfWork, PostsUnitOfWork>();
        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ObjectStorageOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey);

            client = options.UseSsl ? client.WithSSL() : client;
            return client.Build();
        });
        services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();
        services.AddSingleton<InMemoryOutboxSignal>();
        services.AddSingleton<IOutboxSignal>(sp => sp.GetRequiredService<InMemoryOutboxSignal>());
        services.AddSingleton<IIntegrationEventPublisher, KafkaPostCreatedPublisher>();
        services.AddHostedService<ObjectStorageBucketInitializerHostedService>();
        services.AddHostedService<KafkaTopicInitializerHostedService>();
        services.AddHostedService<OutboxPublisherBackgroundService>();

        return services;
    }
}
