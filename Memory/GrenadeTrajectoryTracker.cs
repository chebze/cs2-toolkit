using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Memory;

public sealed class GrenadeTrajectoryTracker
{
    private readonly GrenadeTrajectoryResolver _resolver;
    private readonly GrenadeOverlayOptions _overlayOptions;
    private readonly ILogger<GrenadeTrajectoryTracker> _logger;
    private readonly object _lock = new();

    private GameOffsets? _offsets;
    private MapVisibilityChecker? _mapChecker;
    private GrenadeTrajectorySnapshot _snapshot = new();
    private string _lastStatus = "inactive";
    private DateTime _lastPollLoggedAt;
    private DateTime _lastDrawLoggedAt;
    private string _lastLoggedStatus = string.Empty;

    public GrenadeTrajectoryTracker(
        IOptions<ToolkitOptions> options,
        ILogger<GrenadeTrajectoryTracker> logger)
    {
        _resolver = new GrenadeTrajectoryResolver(options.Value.Grenade);
        _overlayOptions = options.Value.Overlay.GrenadeTrajectory;
        _logger = logger;
    }

    public GrenadeTrajectorySnapshot Snapshot
    {
        get
        {
            lock (_lock)
                return _snapshot;
        }
    }

    public string LastStatus
    {
        get
        {
            lock (_lock)
                return _lastStatus;
        }
    }

    public void Initialize(GameOffsets offsets, MapVisibilityChecker mapChecker)
    {
        _offsets = offsets;
        _mapChecker = mapChecker;
    }

    public void Poll(ProcessMemory memory, MemoryState state)
    {
        if (_offsets is null || _mapChecker is null || !memory.IsAttached || !state.IsInMatch)
        {
            Clear("cleared: not ready or not in match");
            return;
        }

        var result = _resolver.Resolve(
            memory,
            _offsets,
            _mapChecker,
            memory.ClientBase,
            state);

        lock (_lock)
        {
            _snapshot = result.Snapshot;
            _lastStatus = result.Status;
        }

        LogDiagnostics(result);
    }

    public void LogDrawSkip(string reason)
    {
        if (!_overlayOptions.LogDiagnostics)
            return;

        var now = DateTime.UtcNow;
        if ((now - _lastDrawLoggedAt).TotalMilliseconds < _overlayOptions.LogDiagnosticsIntervalMs)
            return;

        _lastDrawLoggedAt = now;
        _logger.LogInformation("[GrenadeDraw] skip: {Reason} lastStatus={Status}", reason, LastStatus);
    }

    public void LogDrawResult(int projectedPoints, int drawnSegments, bool landingVisible)
    {
        if (!_overlayOptions.LogDiagnostics)
            return;

        var now = DateTime.UtcNow;
        if ((now - _lastDrawLoggedAt).TotalMilliseconds < _overlayOptions.LogDiagnosticsIntervalMs)
            return;

        _lastDrawLoggedAt = now;
        _logger.LogInformation(
            "[GrenadeDraw] drew segments={Segments} projected={Projected} landing={LandingVisible} lastStatus={Status}",
            drawnSegments,
            projectedPoints,
            landingVisible,
            LastStatus);
    }

    private void LogDiagnostics(GrenadeTrajectoryDiagnostics result)
    {
        if (!_overlayOptions.LogDiagnostics)
            return;

        var now = DateTime.UtcNow;
        var status = result.Snapshot.IsActive
            ? $"active {result.Status}"
            : result.Status;

        if (string.Equals(status, _lastLoggedStatus, StringComparison.Ordinal)
            && (now - _lastPollLoggedAt).TotalMilliseconds < _overlayOptions.LogDiagnosticsIntervalMs)
            return;

        _lastPollLoggedAt = now;
        _lastLoggedStatus = status;
        _logger.LogInformation("[Grenade] {Status}", status);
    }

    private void Clear(string status)
    {
        lock (_lock)
        {
            _snapshot = new GrenadeTrajectorySnapshot();
            _lastStatus = status;
        }

        LogDiagnostics(new GrenadeTrajectoryDiagnostics { Status = status });
    }
}
