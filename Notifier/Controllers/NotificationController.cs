using Microsoft.AspNetCore.Mvc;
using Notifier.Models;
using Notifier.Services;

namespace Notifier.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationController(NotificationClassifierService notificationClassifierService):ControllerBase
{
    [HttpPost("receive")]
    public async Task<ActionResult> ReceiveNotification([FromBody]Notification notification)
    {
        await notificationClassifierService.ClassifyAndSendAsync(notification);
        
        return Ok();
    }
}