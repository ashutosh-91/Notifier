using System.ComponentModel.DataAnnotations;
using Notifier.Enums;

namespace Notifier.Models;

/// <summary>
/// Model to represent the payload received from a external service.
/// </summary>
/// <param name="Id">A unique Id</param>
/// <param name="Level">Serverity</param>
/// <param name="Message">The message sent by client, we will keep it to a length of 5k for now as we do not want to bloat the context.</param>
public record Notification(
    int Id,
    NotificationLevel Level,
    [Required]
    [StringLength(5000, MinimumLength = 1)]
    string Message);