using System.Net;

namespace Notifier.UnitTests.Fakes;

/// <summary>
/// Captures outgoing requests instead of sending them over the network.
/// </summary>
public sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    public List<string> RequestBodies { get; } = [];

    public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (request.Content is not null)
        {
            RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }

        return new HttpResponseMessage(ResponseStatusCode)
        {
            Content = new StringContent("{}")
        };
    }
}
