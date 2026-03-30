using System.Collections.Concurrent;

namespace Mews.Job.Scheduler.Domain.JobLifecycle;

public sealed class JobTimeoutRetryCache
{
    private readonly ConcurrentDictionary<Guid, int> _cache;

    public JobTimeoutRetryCache()
    {
        _cache = new ();
    }

    public int Increment(Guid id)
    {
        if (_cache.TryGetValue(id, out var value))
        {
            var newValue = value + 1;
            _cache.TryUpdate(id, newValue, value);

            return newValue;
        }

        return _cache.GetOrAdd(id, 1);
    }

    public int Reset(Guid id)
    {
        _ = _cache.TryRemove(id, out _);

        return 0;
    }
}
