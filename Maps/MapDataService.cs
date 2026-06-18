using Cs2Toolkit.Configuration;
using Cs2Toolkit.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Maps;

public sealed class MapDataService
{
    private readonly MapPhysicsParser _parser;
    private readonly MapVisibilityChecker _visibilityChecker;
    private readonly RuntimeGate _runtimeGate;
    private readonly ToolkitOptions _options;
    private readonly ILogger<MapDataService> _logger;

    public MapDataService(
        MapPhysicsParser parser,
        MapVisibilityChecker visibilityChecker,
        RuntimeGate runtimeGate,
        IOptions<ToolkitOptions> options,
        ILogger<MapDataService> logger)
    {
        _parser = parser;
        _visibilityChecker = visibilityChecker;
        _runtimeGate = runtimeGate;
        _options = options.Value;
        _logger = logger;
    }

    public MapVisibilityChecker VisibilityChecker => _visibilityChecker;

    public async Task ParseAllMapsAsync(Action<string> reportProgress, CancellationToken cancellationToken)
    {
        var cacheDirectory = Path.Combine(AppContext.BaseDirectory, _options.Maps.CacheDirectory);
        Directory.CreateDirectory(cacheDirectory);

        var mapsDirectory = ResolveMapsDirectory(out var discoverySource);
        if (mapsDirectory is null)
        {
            var loadedFromCache = TryLoadCachedMaps(cacheDirectory);
            _runtimeGate.SignalMapParsingComplete();

            if (loadedFromCache > 0)
            {
                reportProgress($"Loaded {loadedFromCache} cached maps");
                _logger.LogWarning(
                    "CS2 maps directory not found; continuing with {Count} cached map(s). Set Toolkit:Maps:MapsDirectory to override.",
                    loadedFromCache);
                return;
            }

            reportProgress("Map data unavailable — continuing without collision meshes");
            _logger.LogWarning(
                "CS2 maps directory not found and no cached map meshes exist. Map raycast features will be disabled.");
            return;
        }

        _logger.LogInformation("Located CS2 maps directory via {Source}: {MapsDirectory}", discoverySource, mapsDirectory);

        var vpks = Cs2InstallLocator.EnumerateMapVpks(mapsDirectory).ToList();
        if (vpks.Count == 0)
        {
            var loadedFromCache = TryLoadCachedMaps(cacheDirectory);
            _runtimeGate.SignalMapParsingComplete();
            reportProgress(loadedFromCache > 0
                ? $"Loaded {loadedFromCache} cached maps"
                : "No map VPKs found — continuing without collision meshes");
            _logger.LogWarning("No map VPK files found in {MapsDirectory}", mapsDirectory);
            return;
        }

        var parsed = 0;
        for (var i = 0; i < vpks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var vpkPath = vpks[i];
            var mapName = Path.GetFileNameWithoutExtension(vpkPath);
            if (mapName.EndsWith("_dir", StringComparison.OrdinalIgnoreCase))
                mapName = mapName[..^4];

            reportProgress($"Parsing maps... ({i + 1}/{vpks.Count}) {mapName}");

            var mesh = await Task.Run(() => _parser.ParseMapVpk(vpkPath, cacheDirectory, mapsDirectory), cancellationToken);
            if (mesh is not null)
            {
                _visibilityChecker.RegisterMap(mesh);
                parsed++;
                _logger.LogInformation("Parsed map {MapName} ({TriangleCount} triangles)", mapName, mesh.TriangleCount);
            }
            else
            {
                _logger.LogWarning("Skipped map {MapName}", mapName);
            }
        }

        if (parsed == 0)
        {
            parsed = TryLoadCachedMaps(cacheDirectory);
            _runtimeGate.SignalMapParsingComplete();
            reportProgress(parsed > 0
                ? $"Loaded {parsed} cached maps"
                : "Failed to parse maps — continuing without collision meshes");
            _logger.LogWarning("Failed to parse any map collision meshes from {MapsDirectory}", mapsDirectory);
            return;
        }

        _runtimeGate.SignalMapParsingComplete();
        reportProgress($"Parsed {parsed}/{vpks.Count} maps");
        _logger.LogInformation("Map parsing complete — {Parsed}/{Total} maps ready", parsed, vpks.Count);
    }

    private string? ResolveMapsDirectory(out string? discoverySource)
    {
        discoverySource = null;

        if (!string.IsNullOrWhiteSpace(_options.Maps.MapsDirectory))
        {
            var configured = _options.Maps.MapsDirectory.Trim();
            if (Directory.Exists(configured))
            {
                discoverySource = "appsettings MapsDirectory";
                return configured;
            }

            _logger.LogWarning("Configured maps directory does not exist: {MapsDirectory}", configured);
        }

        var located = Cs2InstallLocator.LocateMapsDirectory();
        if (located is null)
            return null;

        discoverySource = located.DiscoverySource;
        return located.MapsDirectory;
    }

    private int TryLoadCachedMaps(string cacheDirectory)
    {
        if (!Directory.Exists(cacheDirectory))
            return 0;

        var loaded = 0;
        foreach (var cachePath in Directory.EnumerateFiles(cacheDirectory, "*.mapmesh"))
        {
            var mapName = Path.GetFileNameWithoutExtension(cachePath);
            var mesh = _parser.LoadCachedMesh(cachePath);
            if (mesh is null)
                continue;

            _visibilityChecker.RegisterMap(mesh);
            loaded++;
            _logger.LogInformation("Loaded cached map {MapName} ({TriangleCount} triangles)", mapName, mesh.TriangleCount);
        }

        return loaded;
    }
}
