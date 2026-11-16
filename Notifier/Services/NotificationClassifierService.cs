using Microsoft.Extensions.Options;
using Notifier.Enums;
using Notifier.Models;
using Notifier.Services.Interfaces;

namespace Notifier.Services;

public class NotificationClassifierService(IConfiguration configuration, IAiProviderService aiService, IDiscordClientService discordClientService, IOptions<Ai> aiOptions, RollingWindowRateLimiter rateLimiter)
{

    public async Task<NotificationResult> ClassifyAndSendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Level < NotificationLevel.Warning)
        {
            return new NotificationResult(NotificationOutcome.NotForwarded);
        }

        if (!rateLimiter.TrySend(out var retryAfter))
        {
            return new NotificationResult(NotificationOutcome.RateLimited, RetryAfter: retryAfter);
        }
        return new NotificationResult(NotificationOutcome.Forwarded, "message");
        string? message;
        try
        {
            message = await aiService.CompleteChatAsync(aiOptions.Value.SystemPrompts.NotificationClassifier, notification.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new NotificationResult(NotificationOutcome.AiUnavailable);
        }

        try
        {
            await discordClientService.SendMessageAsync(configuration["Discord:ChannelId"], message ?? notification.Message);
        }
        catch (Exception)
        {
            return new NotificationResult(NotificationOutcome.DiscordUnavailable);
        }

        return new NotificationResult(NotificationOutcome.Forwarded, message);
    }
}
