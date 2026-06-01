namespace IntegrationTests.TestInfrastructure;

using System.Net;
using System.Text;

public class FakeGeniusHttpMessageHandler : HttpMessageHandler
{
    private string _searchResponseJson = """{"response":{"hits":[]}}""";
    private string _lyricsPageHtml = "";

    public void SetupSearchResponse(string lyricsUrl) =>
        _searchResponseJson = $$"""
        {
          "response": {
            "hits": [{
              "result": {
                "url": "{{lyricsUrl}}",
                "primary_artist": { "name": "Test Artist" }
              }
            }]
          }
        }
        """;

    public void SetupLyricsPageHtml(string html) => _lyricsPageHtml = html;

    public void SetupNoHits() =>
        _searchResponseJson = """{"response":{"hits":[]}}""";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        if (request.RequestUri!.Host == "api.genius.com")
        {
            response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_searchResponseJson, Encoding.UTF8, "application/json")
            };
        }
        else
        {
            response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_lyricsPageHtml, Encoding.UTF8, "text/html")
            };
        }

        return Task.FromResult(response);
    }
}
