using Notifier.Enums;

namespace Notifier.Models;

/// <summary>
/// Model to represent the payload received from a external service.
/// </summary>
/// <param name="Id">A unique Id</param>
/// <param name="Level">Serverity</param>
/// <param name="TimeStamp">Time in UTC</param>
public record Notification(int Id, NotificationLevel Level,string Message, DateTimeOffset TimeStamp);