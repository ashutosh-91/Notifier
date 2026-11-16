using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Notifier.Enums;
using Notifier.Models;
using Notifier.Services;
using Notifier.UnitTests.MockServices;

namespace Notifier.UnitTests.Services;

[TestFixture]
public class NotificationClassifierServiceTests
{
    private const string SystemPrompt = "classify";
    private const string ChannelId = "42";

    private static (NotificationClassifierService Service, MockAiProviderService Ai, MockDiscordClientService Discord) CreateService(RollingWindowRateLimiter? limiter = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:ChannelId"] = ChannelId
            })
            .Build();

        var aiOptions = Options.Create(new Ai
        {
            BaseUrl = "https://example.test",
            Model = "test-model",
            ApiKey = "test-key",
            SystemPrompts = new SystemPrompts { NotificationClassifier = SystemPrompt }
        });

        var ai = new MockAiProviderService();
        var discord = new MockDiscordClientService();

        return (new NotificationClassifierService(configuration, ai, discord, aiOptions, limiter ?? CreateLimiter()), ai, discord);
    }

    private static RollingWindowRateLimiter CreateLimiter(int limit = 10) =>
        new(limit, TimeSpan.FromMinutes(1));

    [Test]
    public async Task Info_notification_is_not_forwarded()
    {
        var (service, ai, discord) = CreateService();

        var result = await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Info, "just an update"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.NotForwarded));
        Assert.That(ai.Calls, Is.Empty);
        Assert.That(discord.Calls, Is.Empty);
    }

    [TestCase(NotificationLevel.Warning)]
    [TestCase(NotificationLevel.Error)]
    [TestCase(NotificationLevel.Critical)]
    public async Task Warning_or_above_is_classified_then_forwarded(NotificationLevel level)
    {
        var (service, ai, discord) = CreateService();

        var result = await service.ClassifyAndSendAsync(new Notification(1, level, "something happened"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.Forwarded));
        Assert.That(result.Message, Is.EqualTo("classified"));

        Assert.That(ai.Calls, Has.Count.EqualTo(1));
        Assert.That(ai.Calls[0].SystemPrompt, Is.EqualTo(SystemPrompt));
        Assert.That(ai.Calls[0].UserPrompt, Is.EqualTo("something happened"));

        Assert.That(discord.Calls, Has.Count.EqualTo(1));
        Assert.That(discord.Calls[0].ChannelId, Is.EqualTo(ChannelId));
        Assert.That(discord.Calls[0].Message, Is.EqualTo("classified"));
    }

    [Test]
    public async Task Notification_is_dropped_when_rate_limited()
    {
        var (service, ai, discord) = CreateService(CreateLimiter(limit: 1));

        await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Warning, "first"));
        var result = await service.ClassifyAndSendAsync(new Notification(2, NotificationLevel.Warning, "second"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.RateLimited));
        Assert.That(result.RetryAfter, Is.Not.Null);
        Assert.That(ai.Calls, Has.Count.EqualTo(1));
        Assert.That(discord.Calls, Has.Count.EqualTo(1));
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.BadGateway)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    public async Task Ai_failure_is_reported_and_discord_is_not_called(HttpStatusCode statusCode)
    {
        var (service, ai, discord) = CreateService();
        ai.ThrowOnCall = new HttpRequestException("ai failed", null, statusCode);

        var result = await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Warning, "boom"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.AiUnavailable));
        Assert.That(discord.Calls, Is.Empty);
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.BadGateway)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    public async Task Discord_failure_is_reported(HttpStatusCode statusCode)
    {
        var (service, ai, discord) = CreateService();
        discord.ThrowOnCall = new HttpRequestException("discord failed", null, statusCode);

        var result = await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Warning, "boom"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.DiscordUnavailable));
        Assert.That(ai.Calls, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Falls_back_to_the_original_message_when_the_ai_returns_nothing()
    {
        var (service, ai, discord) = CreateService();
        ai.Result = null;

        var result = await service.ClassifyAndSendAsync(new Notification(1, NotificationLevel.Warning, "original"));

        Assert.That(result.Outcome, Is.EqualTo(NotificationOutcome.Forwarded));
        Assert.That(result.Message, Is.Null);
        Assert.That(discord.Calls, Has.Count.EqualTo(1));
        Assert.That(discord.Calls[0].Message, Is.EqualTo("original"));
    }
}
