using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using System.Net.Http.Json;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;

namespace E2ETests.TestInfrastructure;

/// <summary>
/// Fixture for E2E tests against real AWS DynamoDB + real Spotify API.
/// Requires:
///   - AWS credentials via default chain (env vars or pod IAM role)
///   - Spotify__ClientId and Spotify__ClientSecret env vars
/// Run with: dotnet test --filter "Category=E2E"
/// </summary>
public class E2EFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }
    public HttpClient? Client { get; private set; }

    private const string SearchTracksEndpoint = "/searchTracks";
    private const string RegisterEndpoint = "/register";
    private const string LoginEndpoint = "/login";
    public const string SubmitChordsEndpoint = "/tracks";

    private readonly string _runId = Guid.NewGuid().ToString("N")[..8];
    public string TestUserEmail => $"e2e-{_runId}@test.dachord.dev";
    private const string TestUserPassword = "E2eTestPassword123!";

    private string? _token;
    public IReadOnlyList<TrackResponse> DiscoveredTracks { get; private set; } = [];

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.UseSetting(WebHostDefaults.EnvironmentKey, "E2E"));

        Client = Factory.CreateClient();

        // Register test user
        var reg = await Client.PostAsJsonAsync(RegisterEndpoint, new RegisterRequest
        {
            Email = TestUserEmail,
            Password = TestUserPassword
        });
        if (!reg.IsSuccessStatusCode)
            throw new Exception($"E2E register failed {reg.StatusCode}: {await reg.Content.ReadAsStringAsync()}");

        // Discover real tracks from Spotify to use in tests
        var searchResponse = await Client.PostAsJsonAsync(SearchTracksEndpoint,
            new TrackSearchRequest { Query = "Avenged Sevenfold Nightmare" });

        if (!searchResponse.IsSuccessStatusCode)
            throw new Exception($"E2E Spotify search failed {searchResponse.StatusCode}: {await searchResponse.Content.ReadAsStringAsync()}");

        var result = await searchResponse.Content.ReadFromJsonAsync<SearchTracksResponse>();
        DiscoveredTracks = result?.Results ?? [];

        if (DiscoveredTracks.Count == 0)
            throw new Exception("E2E setup: Spotify returned no tracks for Avenged Sevenfold Nightmare");
    }

    public async Task<string> GetTokenAsync()
    {
        if (_token != null) return _token;

        var response = await Client!.PostAsJsonAsync(LoginEndpoint, new LoginRequest
        {
            Email = TestUserEmail,
            Password = TestUserPassword
        });
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"E2E login failed {response.StatusCode}: {body}");

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _token = login?.Token ?? throw new Exception($"E2E login returned empty token. Body: {body}");
        return _token;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (body != null)
            request.Content = JsonContent.Create(body);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Add("Authorization", $"Bearer {token}");
        return await Client!.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendAuthenticatedAsync(HttpMethod method, string endpoint, object? body = null)
        => await SendAsync(method, endpoint, body, await GetTokenAsync());

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        await Task.CompletedTask;
    }
}
