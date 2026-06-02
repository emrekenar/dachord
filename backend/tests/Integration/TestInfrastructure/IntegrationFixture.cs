using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Testcontainers.LocalStack;

using System.Net.Http.Json;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;

namespace IntegrationTests.TestInfrastructure;

public class IntegrationFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    private readonly LocalStackContainer _localStack;
    private IAmazonDynamoDB? _dynamoDb;

    private const string TracksTableName = "dachord-test-tracks";
    private const string UsersTableName = "dachord-test-users";

    private const string RegisterEndpoint = "/register";
    private const string LoginEndpoint = "/login";
    public const string SubmitChordsEndpoint = "/tracks";

    public const string ExistingTrackId = "existingTrackId";
    public const string ExistingTestUserId = "existingUser";
    public const string ExistingTestUserDisplayName = "Test User";
    public const string ExistingTestUserEmail = "testuser@example.com";
    private const string ExistingTestUserPassword = "TestPassword123!";
    private string? ExistingTestUserToken = null;

    public IntegrationFixture()
    {
        _localStack = new LocalStackBuilder("localstack/localstack:3")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _localStack.StartAsync();
        var connectionString = _localStack.GetConnectionString();
        var config = new AmazonDynamoDBConfig { ServiceURL = connectionString };
        _dynamoDb = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), config);

        // Create tracks table: pk (HASH) + sk (RANGE)
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = TracksTableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("pk", ScalarAttributeType.S),
                new AttributeDefinition("sk", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("pk", KeyType.HASH),
                new KeySchemaElement("sk", KeyType.RANGE)
            },
            ProvisionedThroughput = new ProvisionedThroughput(5, 5)
        });

        // Create users table: pk (HASH) + EmailIndex GSI for efficient GetByEmail queries
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = UsersTableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("pk", ScalarAttributeType.S),
                new AttributeDefinition("Email", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("pk", KeyType.HASH)
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "EmailIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Email", KeyType.HASH)
                    },
                    Projection = new Projection { ProjectionType = ProjectionType.ALL },
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                }
            },
            ProvisionedThroughput = new ProvisionedThroughput(5, 5)
        });

        // Seed existing track so SubmitChordsService doesn't fall back to Spotify lookup
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TracksTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue(ExistingTrackId) },
                { "sk", new AttributeValue("METADATA") },
                { "Title", new AttributeValue("Test Track") },
                { "ArtistId", new AttributeValue("artist001") },
                { "ArtistName", new AttributeValue("Test Artist") },
                { "AlbumId", new AttributeValue("album001") },
                { "AlbumName", new AttributeValue("Test Album") }
            }
        });

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("AWS:ServiceURL", connectionString);
                builder.UseSetting("AWS:AccessKey", "test");
                builder.UseSetting("AWS:SecretKey", "test");
                builder.UseSetting("Jwt:Key", "integration-test-secret-key-must-be-at-least-32-chars");
                builder.UseSetting("DynamoDb:TableNamePrefix", "dachord-test-");
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ISearchTracksService, NullSearchTracksService>();
                });
            });
        Client = Factory.CreateClient();

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
        return await Client!.SendAsync(request);
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

        var response = Client!.SendAsync(request).Result;
        var body = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Login failed with {response.StatusCode}. Body: {body}");

        var loginResponse = response.Content.ReadFromJsonAsync<LoginResponse>().Result;
        var token = loginResponse?.Token;
        if (string.IsNullOrEmpty(token))
            throw new Exception($"Login returned empty token. Body: {body}");

        ExistingTestUserToken = token;
        return ExistingTestUserToken;
    }

    public async Task DisposeAsync()
    {
        if (_dynamoDb != null)
        {
            await _dynamoDb.DeleteTableAsync(TracksTableName);
            await _dynamoDb.DeleteTableAsync(UsersTableName);
        }
        await _localStack.DisposeAsync();

        Client?.Dispose();
        Factory?.Dispose();
    }
}
