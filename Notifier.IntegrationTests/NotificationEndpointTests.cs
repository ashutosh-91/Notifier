using System.Net;
using System.Net.Http.Json;

namespace Notifier.IntegrationTests;

[TestFixture]
public class NotificationEndpointTests
{
    private NotifierApiFactory _factory = null!;

    [OneTimeSetUp]
    public void CreateFactory() => _factory = new NotifierApiFactory();

    [OneTimeTearDown]
    public void DisposeFactory() => _factory.Dispose();

    [SetUp]
    public void ResetRecordedCalls()
    {
        _factory.Ai.Calls.Clear();
        _factory.Discord.Calls.Clear();
    }

    [Test]
    public async Task Info_notification_returns_ok_without_notifying_anyone()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(Id: 1, Level: "Info", Message: "just an update"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.Ai.Calls, Is.Empty);
        Assert.That(_factory.Discord.Calls, Is.Empty);
    }

    [TestCase("Warning")]
    [TestCase("Error")]
    [TestCase("Critical")]
    public async Task Warning_or_above_returns_accepted_and_forwards(string level)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(Id: 2, Level: level, Message: "disk almost full"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        Assert.That(_factory.Ai.Calls, Has.Count.EqualTo(1));
        Assert.That(_factory.Ai.Calls[0].SystemPrompt, Is.EqualTo("classify"));
        Assert.That(_factory.Ai.Calls[0].UserPrompt, Is.EqualTo("disk almost full"));

        Assert.That(_factory.Discord.Calls, Has.Count.EqualTo(1));
        Assert.That(_factory.Discord.Calls[0].ChannelId, Is.EqualTo("42"));
    }

    [Test]
    public async Task Empty_message_is_rejected_with_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new NotificationRequest(Id: 3, Level: "Info", Message: ""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Missing_message_is_rejected_with_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new { id = 4, level = "Info" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Unknown_level_is_rejected_with_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/notifications/receive",
            new { id = 5, level = "Bogus", message = "hi" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private sealed record NotificationRequest(int Id, string Level, string Message);
}
