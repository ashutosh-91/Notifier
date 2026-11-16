using Notifier.Services.Interfaces;

namespace Notifier.IntegrationTests.Fakes;

/// <summary>
/// Stands in for the real Discord client so integration tests never call out to a network.
/// </summary>
public sealed class RecordingDiscordClientService : IDiscordClientService
{
    public List<(string ChannelId, string Message)> Calls { get; } = [];

    public Exception? ThrowOnCall { get; set; }

    public Task SendMessageAsync(string channelId, string message)
    {
        Calls.Add((channelId, message));
        return ThrowOnCall is null ? Task.CompletedTask : Task.FromException(ThrowOnCall);
    }
}
