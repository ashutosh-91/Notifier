using System.Net;
using Notifier.Models;
using Notifier.Services.Interfaces;

namespace Notifier.Services;

public class DiscordClientService : IDiscordClientService
{
    private readonly HttpClient client;
    public DiscordClientService(HttpClient httpClientFactory, IConfiguration configuration)
    {
        client = httpClientFactory;
    }
  
    public async Task SendMessageAsync(string channelId, string message)
    {
        //Check if channel Id and message not null
        var payload = new
        {
            content = message
        };

      
        var response = await client.PostAsJsonAsync(
            $"channels/{channelId}/messages",
            payload
        );
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
}