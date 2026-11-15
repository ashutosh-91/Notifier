using Notifier.Services.Interfaces;

namespace Notifier.UnitTests.Fakes;

public sealed class FakeDiscordClientService : IDiscordClientService
{
    public List<(string ChannelId, string Message)> Calls { get; } = [];

    public Task SendMessageAsync(string channelId, string message)
    {
        Calls.Add((channelId, message));
        return Task.CompletedTask;
    }
}
