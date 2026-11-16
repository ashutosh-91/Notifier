namespace Notifier.Services;

/// <summary>
/// Allows at most <paramref name="limit"/> sends in any <paramref name="window"/> of time,
/// no matter how they are spaced out.
/// </summary>
public sealed class RollingWindowRateLimiter(int limit, TimeSpan window)
{
    private readonly Queue<DateTimeOffset> _sendTimes = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Records a send when there is still room in the window.
    /// </summary>
    /// <param name="retryAfter">How long until a send is allowed again;</param>
    /// <returns><c>true</c> when the send is allowed.</returns>
    public bool TrySend(out TimeSpan retryAfter)
    {
        var now = DateTimeOffset.UtcNow;

        lock (_lock)
        {
            // Forget sends that happened more than "window" ago.
            while (_sendTimes.Count > 0 && now - _sendTimes.Peek() >= window)
            {
                _sendTimes.Dequeue();
            }

            // Already sent "limit" times inside the window?
            if (_sendTimes.Count >= limit)
            {
                retryAfter = _sendTimes.Peek() + window - now;
                return false;
            }

            // There is room
            _sendTimes.Enqueue(now);
            retryAfter = TimeSpan.Zero;
            return true;
        }
    }
}
