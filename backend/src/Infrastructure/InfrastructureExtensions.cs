using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Domain.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Persistence;

namespace Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamoDbSection = configuration.GetSection("DynamoDb");
        var options = new DynamoDbOptions();
        dynamoDbSection.Bind(options);
        services.Configure<DynamoDbOptions>(dynamoDbSection);

        var awsServiceUrl = configuration["AWS:ServiceURL"];
        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];
        var awsRegion = configuration["AWS:Region"] ?? "eu-central-1";

        services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            // Local/test: ServiceURL is set → explicit credentials against LocalStack
            if (!string.IsNullOrEmpty(awsServiceUrl))
            {
                return new AmazonDynamoDBClient(
                    new Amazon.Runtime.BasicAWSCredentials(awsAccessKey ?? "test", awsSecretKey ?? "test"),
                    new AmazonDynamoDBConfig { ServiceURL = awsServiceUrl }
                );
            }

            // Real AWS: explicit ServiceURL bypasses SDK v4 endpoint resolution which can hang
            return new AmazonDynamoDBClient(
                new AmazonDynamoDBConfig
                {
                    ServiceURL = $"https://dynamodb.{awsRegion}.amazonaws.com",
                    AuthenticationRegion = awsRegion,
                    MaxErrorRetry = 2
                }
            );
        });

        services.AddSingleton<IDynamoDBContext>(sp =>
            new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => sp.GetRequiredService<IAmazonDynamoDB>())
                .ConfigureContext(cfg => cfg.TableNamePrefix = options.TableNamePrefix)
                .Build()
        );

        services.AddSingleton<ITrackRepository, TrackRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
        return services;
    }
}
