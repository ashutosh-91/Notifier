using Microsoft.Extensions.Configuration;
using Notifier.Enums;
using Notifier.Models;
using Notifier.Services;
using Notifier.UnitTests.Fakes;

namespace Notifier.UnitTests.Services;

public class NotificationClassifierServiceTests
{
    private const string SystemPrompt = "classify";
    private const string ChannelId = "42";

    private static (NotificationClassifierService Service, FakeAiProviderService Ai, FakeDiscordClientService Discord) CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:ChannelId"] = ChannelId,
                ["Ai:SystemPrompts:NotificationClassifier"] = SystemPrompt
            })
            .Build();

        var ai = new FakeAiProviderService();
        var discord = new FakeDiscordClientService();

        return (new NotificationClassifierService(configuration, ai, discord), ai, discord);
    }

    [Fact]
    public async Task ClassifyAndSendAsync_does_nothing_for_info_notifications()
    {
        var (service, ai, discord) = CreateService();

        await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Info, "just an update"));

        Assert.Empty(ai.Calls);
        Assert.Empty(discord.Calls);
    }

    [Theory]
    [InlineData(NotificationLevel.Warning)]
    [InlineData(NotificationLevel.Error)]
    [InlineData(NotificationLevel.Critical)]
    public async Task ClassifyAndSendAsync_classifies_then_sends_for_warning_or_above(NotificationLevel level)
    {
        var (service, ai, discord) = CreateService();

        await service.ClassifyAndSendAsync(new Notification(1, level, "something happened"));

        var aiCall = Assert.Single(ai.Calls);
        Assert.Equal(SystemPrompt, aiCall.SystemPrompt);
        Assert.Equal("something happened", aiCall.UserPrompt);

        var discordCall = Assert.Single(discord.Calls);
        Assert.Equal(ChannelId, discordCall.ChannelId);
        Assert.Equal("Hello", discordCall.Message);
    }
}
