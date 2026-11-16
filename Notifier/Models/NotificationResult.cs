namespace Notifier.Models;

/// <summary>What happened to an incoming notification.</summary>
public enum NotificationOutcome
{
    /// <summary>Below the forwarding threshold; nothing was sent.</summary>
    NotForwarded,

    /// <summary>Classified and delivered downstream.</summary>
    Forwarded,

    /// <summary>The outbound rate limit was reached.</summary>
    RateLimited,

    /// <summary>Classification (the AI provider) failed.</summary>
    AiUnavailable,

    /// <summary>Delivery (Discord) failed.</summary>
    DiscordUnavailable
}

/// <summary>The result of processing a notification.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Message">The generated message, when one was produced.</param>
/// <param name="RetryAfter">How long to wait before retrying, when rate limited.</param>
public sealed record NotificationResult(NotificationOutcome Outcome, string? Message = null, TimeSpan? RetryAfter = null);
