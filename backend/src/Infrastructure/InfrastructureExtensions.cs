using Amazon.DynamoDBv2;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Infrastructure.Configuration;
using Domain.Interfaces;
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

        var awsOptions = configuration.GetAWSOptions(); 
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddSingleton<ITrackRepository, TrackRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        
        return services;
    }
}
