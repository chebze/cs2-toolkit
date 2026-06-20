using System.Text.Json;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class RadarState : IRadarSnapshotProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _lock = new();
    private RadarSnapshot _snapshot = RadarSnapshot.Idle;
    private long _version;

    public event Action<long>? Changed;

    public long Version
    {
        get
        {
            lock (_lock)
                return _version;
        }
    }

    public RadarSnapshot GetSnapshot()
    {
        lock (_lock)
            return _snapshot;
    }

    public string GetSnapshotJson()
    {
        lock (_lock)
            return JsonSerializer.Serialize(_snapshot, JsonOptions);
    }

    public void Update(RadarSnapshot snapshot)
    {
        long version;
        lock (_lock)
        {
            _snapshot = snapshot;
            _version++;
            version = _version;
        }

        Changed?.Invoke(version);
    }
}
