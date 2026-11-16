namespace Notifier.Services.Interfaces;

public interface IDiscordClientService
{
    /// <summary>
    /// Sends message to the Discord channel async.
    /// </summary>
    /// <param name="channelId">Discord ChannelId.</param>
    /// <param name="message">Message to be sent.</param>
    /// <returns></returns>
    Task SendMessageAsync(string channelId, string message);
}
