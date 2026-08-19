using System.Collections.Concurrent;

namespace AbsurdCourt.Api.RealTime;

public sealed class HubInvocationRateLimiter
{
    private readonly ConcurrentDictionary<string, Counter> counters = new();

    public bool TryConsume(string key, int limit, TimeSpan window)
    {
        var counter = counters.GetOrAdd(key, static _ => new Counter());
        lock (counter)
        {
            var now = DateTimeOffset.UtcNow;
            if (now - counter.WindowStarted >= window)
            {
                counter.WindowStarted = now;
                counter.Count = 0;
            }

            if (counter.Count >= limit) return false;
            counter.Count++;
            return true;
        }
    }

    private sealed class Counter
    {
        public DateTimeOffset WindowStarted { get; set; } = DateTimeOffset.UtcNow;
        public int Count { get; set; }
    }
}
