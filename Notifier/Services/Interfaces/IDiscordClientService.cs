namespace Notifier.Services.Interfaces;

public interface IDiscordClientService
{
    Task SendMessageAsync(string channelId, string message);
}
