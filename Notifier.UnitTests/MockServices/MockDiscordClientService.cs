using Notifier.Services.Interfaces;

namespace Notifier.UnitTests.MockServices;

public sealed class MockDiscordClientService : IDiscordClientService
{
    public List<(string ChannelId, string Message)> Calls { get; } = [];

    public Exception? ThrowOnCall { get; set; }

    public Task SendMessageAsync(string channelId, string message)
    {
        Calls.Add((channelId, message));
        return ThrowOnCall is null ? Task.CompletedTask : Task.FromException(ThrowOnCall);
    }
}
