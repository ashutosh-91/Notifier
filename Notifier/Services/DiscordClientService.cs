using System.Net;
using Notifier.Models;

namespace Notifier.Services;

public class DiscordClientService
{
    private readonly HttpClient client;
    public DiscordClientService(HttpClient httpClientFactory, IConfiguration configuration)
    {
        client = httpClientFactory;
    }
  
    public async Task SendMessageAsync(string channelId, string message)
    {
        //Check if channel Id and message not null
        

        var response = await client.PostAsJsonAsync(
            $"channels/{channelId}/messages",
            message
        );

        response.EnsureSuccessStatusCode();
    }
}