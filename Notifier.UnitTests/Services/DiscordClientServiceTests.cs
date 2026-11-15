using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Notifier.Services;
using Notifier.UnitTests.Fakes;

namespace Notifier.UnitTests.Services;

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

    [Fact]
    public async Task SendMessageAsync_posts_to_the_channel_messages_endpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://discord.com/api/v10/channels/42/messages", request.RequestUri!.ToString());
    }

    [Fact]
    public async Task SendMessageAsync_sends_the_message_as_content()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        var body = Assert.Single(handler.RequestBodies);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("hi", document.RootElement.GetProperty("content").GetString());
    }

    [Fact]
    public async Task SendMessageAsync_sends_json_content()
    {
        var handler = new RecordingHttpMessageHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync("42", "hi");

        var request = Assert.Single(handler.Requests);
        Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
    }
}
