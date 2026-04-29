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

        services.AddSingleton<IAmazonDynamoDB>(sp =>
            new AmazonDynamoDBClient(
                new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey),
                new AmazonDynamoDBConfig { ServiceURL = awsServiceUrl }
            )
        );

        services.AddSingleton<IDynamoDBContext>(sp =>
            new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => sp.GetRequiredService<IAmazonDynamoDB>())
                .Build()
        );

        services.AddSingleton<ITrackRepository, TrackRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        return services;
    }
}
