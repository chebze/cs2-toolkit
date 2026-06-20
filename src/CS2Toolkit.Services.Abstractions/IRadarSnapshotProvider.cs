using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IRadarSnapshotProvider
{
    long Version { get; }

    event Action<long>? Changed;

    RadarSnapshot GetSnapshot();

    string GetSnapshotJson();
}
