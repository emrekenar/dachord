using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Testcontainers.LocalStack;

using Infrastructure.Configuration;
using System.Net.Http.Json;
using Application.Requests;
using Application.Responses;

namespace IntegrationTests.TestInfrastructure;

public class IntegrationFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    private readonly LocalStackContainer _localStack;
    private IAmazonDynamoDB? _dynamoDb;

    private const string TableName = "Tracks";

    private const string RegisterEndpoint = "/register";
    private const string LoginEndpoint = "/login";
    private const string SubmitChordsEndpoint = "/tracks";

    private const string ExistingTestUserEmail = "testuser@example.com";
    private const string ExistingTestUserPassword = "TestPassword123!";
    private string? ExistingTestUserToken = null;

    public IntegrationFixture()
    {
        _localStack = new LocalStackBuilder("localstack/localstack:latest")
            .WithPortBinding(4566)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start LocalStack and set up DynamoDB
        await _localStack.StartAsync();
        var config = new AmazonDynamoDBConfig { ServiceURL = _localStack.GetConnectionString() };
        _dynamoDb = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), config);
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = TableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("Id", ScalarAttributeType.S),
                new AttributeDefinition("Type", ScalarAttributeType.S),
                new AttributeDefinition("Email", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("Id", KeyType.HASH),
                new KeySchemaElement("Type", KeyType.RANGE)
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "EmailIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Email", KeyType.HASH),
                        new KeySchemaElement("Type", KeyType.RANGE)
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                }
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

        // Register a test user and obtain a token for authenticated requests
        var registerRequest = new RegisterRequest
        {
            Email = ExistingTestUserEmail,
            Password = ExistingTestUserPassword
        };
        await Client.PostAsJsonAsync(RegisterEndpoint, registerRequest);
    }

    public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string endpoint, object? content = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (content != null)
        {
            request.Content = JsonContent.Create(content);
        }
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("Authorization", $"Bearer {token}");
        }
        return await Client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendRequestAsAuthenticatedUserAsync(HttpMethod method, string endpoint, object? content = null)
    {
        var token = GetAuthenticatedToken();
        return await SendRequestAsync(method, endpoint, content, token);
    }

    private string GetAuthenticatedToken()
    {
        if (ExistingTestUserToken != null)
            return ExistingTestUserToken;

        var loginRequest = new LoginRequest
        {
            Email = ExistingTestUserEmail,
            Password = ExistingTestUserPassword
        };
        var request = new HttpRequestMessage(HttpMethod.Post, LoginEndpoint)
        {
            Content = JsonContent.Create(loginRequest),
        };

        var response = Client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();

        var loginResponse = response.Content.ReadFromJsonAsync<LoginResponse>().Result;
        ExistingTestUserToken = loginResponse?.Token ?? throw new Exception("Failed to obtain token");
        return ExistingTestUserToken;
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
