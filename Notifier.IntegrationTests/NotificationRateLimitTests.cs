using System.Net;
using System.Net.Http.Json;

namespace Notifier.IntegrationTests;

[TestFixture]
public class NotificationRateLimitTests
{
    [Test]
    public async Task Second_warning_is_rejected_with_429_and_retry_after()
    {
        using var factory = new NotifierApiFactory { RateLimitTokens = 1 };
        var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(1, "Warning", "first"));
        var second = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(2, "Warning", "second"));

        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(second.Headers.RetryAfter, Is.Not.Null);
        Assert.That(factory.Ai.Calls, Has.Count.EqualTo(1));
        Assert.That(factory.Discord.Calls, Has.Count.EqualTo(1));
    }

    private sealed record NotificationRequest(int Id, string Level, string Message);
}
