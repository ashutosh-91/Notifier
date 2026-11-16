using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Notifier.Services;
using Notifier.UnitTests.MockServices;

namespace Notifier.UnitTests.Services;

[TestFixture]
public class DiscordClientServiceTests
{
    private static DiscordClientService CreateService(RecordingHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://discord.com/api/v10/")
        };
        var configuration = new ConfigurationBuilder().Build();

        return new DiscordClientService(httpClient, configuration);
    }

    [Test]
    public async Task SendMessageAsync_posts_to_the_channel_messages_endpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        Assert.That(handler.Requests, Has.Count.EqualTo(1));
        Assert.That(handler.Requests[0].Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(handler.Requests[0].RequestUri!.ToString(),
            Is.EqualTo("https://discord.com/api/v10/channels/42/messages"));
    }

    [Test]
    public async Task SendMessageAsync_sends_the_message_as_content()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        Assert.That(handler.RequestBodies, Has.Count.EqualTo(1));
        using var document = JsonDocument.Parse(handler.RequestBodies[0]);
        Assert.That(document.RootElement.GetProperty("content").GetString(), Is.EqualTo("hi"));
    }

    [Test]
    public async Task SendMessageAsync_sends_json_content()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        Assert.That(handler.Requests, Has.Count.EqualTo(1));
        Assert.That(handler.Requests[0].Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task SendMessageAsync_throws_when_discord_returns_an_error()
    {
        var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.BadRequest };
        var service = CreateService(handler);

        Assert.ThrowsAsync<HttpRequestException>(async () => await service.SendMessageAsync("42", "hi"));
    }
}
