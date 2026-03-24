using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Testcontainers.LocalStack;

using Infrastructure.Configuration;

namespace IntegrationTests.TestInfrastructure;

public class IntegrationFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    private readonly LocalStackContainer _localStack;
    private IAmazonDynamoDB? _dynamoDb;

    private const string TableName = "Tracks";

    public IntegrationFixture()
    {
        _localStack = new LocalStackBuilder("localstack/localstack:latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _localStack.StartAsync();
        var config = new AmazonDynamoDBConfig { ServiceURL = _localStack.GetConnectionString() };
        _dynamoDb = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), config);
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = TableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("Id", ScalarAttributeType.S),
                new AttributeDefinition("Type", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("Id", KeyType.HASH),
                new KeySchemaElement("Type", KeyType.RANGE)
            },
            ProvisionedThroughput = new ProvisionedThroughput(5, 5)
        });
        var options = Options.Create(new DynamoDbOptions { TableName = TableName });

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json");
                    config.AddJsonFile("appsettings.Development.json", optional: true);
                });
            });
        Client = Factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_dynamoDb != null)
            await _dynamoDb.DeleteTableAsync(TableName);
        await _localStack.DisposeAsync();

        Client?.Dispose();
        Factory?.Dispose();
        await Task.CompletedTask;
    }
}
