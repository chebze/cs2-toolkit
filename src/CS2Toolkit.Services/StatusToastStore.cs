using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class StatusToastStore : IStatusToastPublisher
{
    private readonly object _lock = new();
    private readonly List<StatusToast> _timed = [];
    private StatusToast? _persistent;

    public bool HasActive
    {
        get
        {
            lock (_lock)
            {
                PruneExpired(DateTimeOffset.UtcNow);
                return _persistent is not null || _timed.Count > 0;
            }
        }
    }

    public IReadOnlyList<StatusToast> GetActive()
    {
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;
            PruneExpired(now);

            var active = new List<StatusToast>();
            if (_persistent is not null)
                active.Add(_persistent);

            active.AddRange(_timed);
            return active;
        }
    }

    public void Publish(string message, TimeSpan? duration = null, uint colorArgb = 0)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var now = DateTimeOffset.UtcNow;
        var expiresAt = duration.HasValue ? now + duration.Value : (DateTimeOffset?)null;

        lock (_lock)
        {
            PruneExpired(now);
            _timed.Add(new StatusToast(message.Trim(), now, expiresAt, colorArgb));
        }
    }

    public void SetPersistent(string message, uint colorArgb = 0)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        lock (_lock)
        {
            _persistent = new StatusToast(message.Trim(), DateTimeOffset.UtcNow, null, colorArgb);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _persistent = null;
            _timed.Clear();
        }
    }

    public void ClearPersistent()
    {
        lock (_lock)
            _persistent = null;
    }

    private void PruneExpired(DateTimeOffset now)
    {
        _timed.RemoveAll(toast => toast.ExpiresAt is not null && toast.ExpiresAt <= now);
    }
}
