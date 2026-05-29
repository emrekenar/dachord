using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.TestInfrastructure;
using Application.Requests;

namespace IntegrationTests.WebApi;

public class RegisterIntegrationTests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _fixture;
    private static readonly string Endpoint = "/register";

    public RegisterIntegrationTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_WithValidCredentials_ShouldReturnOk()
    {
        var request = new RegisterRequest
        {
            Email = $"new-{Guid.NewGuid():N}@test.com",
            Password = "ValidPassword123!"
        };

        var response = await _fixture.SendRequestAsync(HttpMethod.Post, Endpoint, request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Email = IntegrationFixture.ExistingTestUserEmail,
            Password = "AnyPassword123!"
        };

        var response = await _fixture.SendRequestAsync(HttpMethod.Post, Endpoint, request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
