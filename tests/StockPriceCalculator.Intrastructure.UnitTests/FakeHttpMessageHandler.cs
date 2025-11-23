using System.Net;
using System.Net.Http.Json;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly object _responseObject;
    private readonly HttpStatusCode _statusCode;

    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(object responseObject, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseObject = responseObject;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;

        var response = new HttpResponseMessage(_statusCode);

        if (_statusCode == HttpStatusCode.OK)
        {
            // 用 JsonContent 建立假 response body
            response.Content = JsonContent.Create(_responseObject);
        }

        return Task.FromResult(response);
    }
}
