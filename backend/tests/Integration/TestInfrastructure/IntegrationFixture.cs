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
using Domain.Models.Track;

namespace IntegrationTests.TestInfrastructure;

file class NullSearchTracksService : ISearchTracksService
{
    public Task<SearchTracksResponse> QueryAsync(TrackSearchRequest request) => Task.FromResult(new SearchTracksResponse([]));
    public Task<Track?> GetTrackAsync(string id) => Task.FromResult<Track?>(null);
    public Task<SearchTracksResponse> GetTracksInAlbum(string albumId) => Task.FromResult(new SearchTracksResponse([]));
    public Task<SearchTracksResponse> GetTracksFromArtist(string artistId) => Task.FromResult(new SearchTracksResponse([]));
}

public class IntegrationFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    private readonly LocalStackContainer _localStack;
    private IAmazonDynamoDB? _dynamoDb;

    private const string TracksTableName = "Tracks";
    private const string UsersTableName = "Users";

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
        _localStack = new LocalStackBuilder("localstack/localstack:latest")
            .WithPortBinding(4566)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _localStack.StartAsync();
        var connectionString = _localStack.GetConnectionString();
        var config = new AmazonDynamoDBConfig { ServiceURL = connectionString };
        _dynamoDb = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), config);

        // Create tracks table: TrackId (HASH) + SK (RANGE) — matches TrackItem entity
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = TracksTableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("TrackId", ScalarAttributeType.S),
                new AttributeDefinition("SK", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("TrackId", KeyType.HASH),
                new KeySchemaElement("SK", KeyType.RANGE)
            },
            ProvisionedThroughput = new ProvisionedThroughput(5, 5)
        });

        // Create users table: Id (HASH) only — matches UserItem entity (no sort key)
        await _dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = UsersTableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("Id", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("Id", KeyType.HASH)
            },
            ProvisionedThroughput = new ProvisionedThroughput(5, 5)
        });

        // Seed existing track so SubmitChordsService doesn't fall back to Spotify lookup
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TracksTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "TrackId", new AttributeValue(ExistingTrackId) },
                { "SK", new AttributeValue("METADATA") },
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
