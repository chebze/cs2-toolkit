using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Mapping;
using CS2Toolkit.Game.Offsets;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Game.Services;

internal sealed class GameMemoryLoop : BackgroundService
{
    private readonly ProcessMemory _memory;
    private readonly OffsetDownloader _offsetDownloader;
    private readonly GameStatePublisher _publisher;
    private readonly MapVisibilityService _mapVisibility;
    private readonly MapVisibilityChecker _mapChecker;
    private readonly GrenadeSimulationOptions _grenadeOptions;
    private readonly ToolkitHostSettings _options;
    private readonly ILogger<GameMemoryLoop> _logger;
    private string? _activeMapName;

    public GameMemoryLoop(
        ProcessMemory memory,
        OffsetDownloader offsetDownloader,
        GameStatePublisher publisher,
        MapVisibilityService mapVisibility,
        MapVisibilityChecker mapChecker,
        IOptions<ToolkitHostSettings> options,
        ILogger<GameMemoryLoop> logger)
    {
        _memory = memory;
        _offsetDownloader = offsetDownloader;
        _publisher = publisher;
        _mapVisibility = mapVisibility;
        _mapChecker = mapChecker;
        _grenadeOptions = GrenadeSimulationOptionsFactory.FromSettings(options.Value.Grenade);
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_offsetDownloader.Offsets is null && !stoppingToken.IsCancellationRequested)
            await Task.Delay(50, stoppingToken);

        if (_offsetDownloader.Offsets is null)
            return;

        var factory = new GameSnapshotFactory(
            _memory,
            _offsetDownloader.Offsets,
            _mapChecker,
            _grenadeOptions,
            _options.Clairvoyance);
        var intervalMs = Math.Max(1, _options.MemoryReadIntervalMs);
        _logger.LogInformation("Game memory loop started — interval {Interval}ms", intervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
        var lastSummaryLog = DateTimeOffset.MinValue;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = factory.Create();
                _publisher.Publish(snapshot);
                UpdateActiveMap(snapshot.MapName);

                if (_memory.IsAttached && DateTimeOffset.UtcNow - lastSummaryLog > TimeSpan.FromSeconds(1))
                {
                    lastSummaryLog = DateTimeOffset.UtcNow;
                    LogSnapshotSummary(snapshot);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Memory read failed");
            }
        }
    }

    private void UpdateActiveMap(string? mapName)
    {
        if (string.Equals(_activeMapName, mapName, StringComparison.OrdinalIgnoreCase))
            return;

        _activeMapName = mapName;
        _mapVisibility.SetActiveMap(mapName);
    }

    private void LogSnapshotSummary(GameSnapshot snapshot)
    {
        var weapon = snapshot.LocalPlayer?.ActiveWeaponName ?? "none";
        _logger.LogInformation(
            "Snapshot: map={Map} players={PlayerCount} localWeapon={Weapon} inMatch={InMatch}",
            snapshot.MapName ?? "unknown",
            snapshot.Players.Count,
            weapon,
            snapshot.IsInMatch);
    }
}
