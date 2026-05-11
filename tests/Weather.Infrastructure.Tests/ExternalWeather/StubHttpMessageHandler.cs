using System.Net;

namespace Weather.Infrastructure.Tests.ExternalWeather;

internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
{
    public Uri? RequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;

        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody)
        });
    }
}
