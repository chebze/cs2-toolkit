using CS2Toolkit.API.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.API.Radar;

internal sealed class RadarStreamSource(IRadarSnapshotProvider inner) : IRadarStreamSource
{
    public long Version => inner.Version;

    public event Action<long>? Changed
    {
        add => inner.Changed += value;
        remove => inner.Changed -= value;
    }

    public RadarSnapshot GetSnapshot() => inner.GetSnapshot();

    public string GetSnapshotJson() => inner.GetSnapshotJson();
}
