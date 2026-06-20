using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.API.Abstractions;

/// <summary>
/// Radar snapshot source for HTTP snapshot and SSE stream endpoints.
/// Implemented by adapting <see cref="IRadarSnapshotProvider"/> at the composition root.
/// </summary>
public interface IRadarStreamSource : IRadarSnapshotProvider;
