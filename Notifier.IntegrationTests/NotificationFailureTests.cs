using System.Net;
using System.Net.Http.Json;

namespace Notifier.IntegrationTests;

[TestFixture]
public class NotificationFailureTests
{
    [Test]
    public async Task Returns_bad_gateway_when_the_ai_provider_fails()
    {
        using var factory = new NotifierApiFactory();
        factory.Ai.ThrowOnCall = new HttpRequestException("ai down");
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(1, "Warning", "boom"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(factory.Discord.Calls, Is.Empty);
    }

    [Test]
    public async Task Returns_bad_gateway_when_discord_fails()
    {
        using var factory = new NotifierApiFactory();
        factory.Discord.ThrowOnCall = new HttpRequestException("discord down");
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(1, "Warning", "boom"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(factory.Ai.Calls, Has.Count.EqualTo(1));
    }

    private sealed record NotificationRequest(int Id, string Level, string Message);
}
