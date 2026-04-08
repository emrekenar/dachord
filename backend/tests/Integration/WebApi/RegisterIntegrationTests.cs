using IntegrationTests.TestInfrastructure;

namespace IntegrationTests.WebApi;

public class RegisterIntegrationTests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _fixture;

    public RegisterIntegrationTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }
}