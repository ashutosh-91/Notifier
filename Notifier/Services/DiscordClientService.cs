using Notifier.Services.Interfaces;

namespace Notifier.Services;

public class DiscordClientService : IDiscordClientService
{
    private readonly HttpClient _client;
    public DiscordClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _client = httpClient;
    }

    public async Task SendMessageAsync(string channelId, string message)
    {
        var payload = new
        {
            content = message
        };

        var response = await _client.PostAsJsonAsync(
            $"channels/{channelId}/messages",
            payload
        );

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Discord responded with {(int)response.StatusCode} ({response.StatusCode}): {body}");
        }
    }
}
