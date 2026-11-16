using Microsoft.AspNetCore.Mvc;
using Notifier.Models;
using Notifier.Services;

namespace Notifier.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationController(NotificationClassifierService notificationClassifierService) : ControllerBase
{
    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveNotification([FromBody] Notification notification, CancellationToken cancellationToken)
    {
        var result = await notificationClassifierService.ClassifyAndSendAsync(notification, cancellationToken);

        return result.Outcome switch
        {
            NotificationOutcome.Forwarded => Accepted(new { status = "forwarded", message = result.Message }),
            NotificationOutcome.NotForwarded => Ok(new { status = "ignored" }),
            NotificationOutcome.RateLimited => TooManyRequests(result.RetryAfter),
            NotificationOutcome.AiUnavailable => Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "AI provider unavailable",
                detail: "The notification could not be classified. Try again later."),
            NotificationOutcome.DiscordUnavailable => Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Discord unavailable",
                detail: "The notification could not be delivered. Try again later."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private IActionResult TooManyRequests(TimeSpan? retryAfter)
    {
        if (retryAfter is { } value)
        {
            Response.Headers.RetryAfter = ((int)Math.Ceiling(value.TotalSeconds)).ToString();
        }

        return Problem(
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Rate limit exceeded",
            detail: "The outbound limit of 10 notifications per minute was reached. Retry after the indicated delay.");
    }
}
