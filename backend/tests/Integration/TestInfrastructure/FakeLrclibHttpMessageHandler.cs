namespace IntegrationTests.TestInfrastructure;

using System.Net;
using System.Text;
using System.Text.Json;

public class FakeLrclibHttpMessageHandler : HttpMessageHandler
{
    private HttpResponseMessage _response = OkResponse("Line one\nLine two");

    public void SetupLyrics(string plainLyrics) =>
        _response = OkResponse(plainLyrics);

    public void SetupNotFound() =>
        _response = new HttpResponseMessage(HttpStatusCode.NotFound);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        Task.FromResult(_response);

    private static HttpResponseMessage OkResponse(string plainLyrics) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { plainLyrics }),
                Encoding.UTF8, "application/json")
        };
}
