using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Offsets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class LiveConfigApplier : IHostedService
{
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly ConfigManager _configManager;
    private readonly RcsState _rcsState;
    private readonly TbState _tbState;
    private readonly EnemyEspState _enemyEspState;
    private readonly SoundEspState _soundEspState;
    private readonly AimHelperState _aimHelperState;
    private readonly RecoilCompensator _recoilCompensator;
    private readonly Triggerbot _triggerbot;
    private readonly AimHelper _aimHelper;
    private readonly OverlayStyleState _overlayStyle;
    private readonly OffsetDownloader _offsetDownloader;
    private readonly MapDataService _mapDataService;
    private readonly ViewMatrixHolder _viewMatrixHolder;
    private readonly ILogger<LiveConfigApplier> _logger;

    public LiveConfigApplier(
        RuntimeConfigProvider runtimeConfig,
        ConfigManager configManager,
        RcsState rcsState,
        TbState tbState,
        EnemyEspState enemyEspState,
        SoundEspState soundEspState,
        AimHelperState aimHelperState,
        RecoilCompensator recoilCompensator,
        Triggerbot triggerbot,
        AimHelper aimHelper,
        OverlayStyleState overlayStyle,
        OffsetDownloader offsetDownloader,
        MapDataService mapDataService,
        ViewMatrixHolder viewMatrixHolder,
        ILogger<LiveConfigApplier> logger)
    {
        _runtimeConfig = runtimeConfig;
        _configManager = configManager;
        _rcsState = rcsState;
        _tbState = tbState;
        _enemyEspState = enemyEspState;
        _soundEspState = soundEspState;
        _aimHelperState = aimHelperState;
        _recoilCompensator = recoilCompensator;
        _triggerbot = triggerbot;
        _aimHelper = aimHelper;
        _overlayStyle = overlayStyle;
        _offsetDownloader = offsetDownloader;
        _mapDataService = mapDataService;
        _viewMatrixHolder = viewMatrixHolder;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runtimeConfig.ConfigChanged += ApplyAll;
        ApplyAll();
        _logger.LogInformation("LiveConfigApplier started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _runtimeConfig.ConfigChanged -= ApplyAll;
        return Task.CompletedTask;
    }

    private void ApplyAll()
    {
        var options = _runtimeConfig.Current;
        var settings = _runtimeConfig.ActiveSettings;

        _tbState.Initialize(options.Tb);
        _rcsState.InitializeFromConfig(options.Rcs);
        _enemyEspState.InitializeFromProfile(settings.EnemyEsp);
        _soundEspState.Initialize(options.SoundEsp);
        _aimHelperState.Initialize(options.AimHelper);
        _overlayStyle.Update(settings);

        if (_offsetDownloader.Offsets is not null)
        {
            _recoilCompensator.Initialize(_offsetDownloader.Offsets, options.Rcs);
            _triggerbot.Initialize(_offsetDownloader.Offsets, options.Tb, _mapDataService.VisibilityChecker);
            _aimHelper.Initialize(
                _offsetDownloader.Offsets,
                options.AimHelper,
                _mapDataService.VisibilityChecker,
                _viewMatrixHolder);
        }

        _logger.LogInformation(
            "Applied live config for profile {ProfileName}",
            _runtimeConfig.ActiveProfile.Name);
    }
}

public sealed class OverlayStyleState
{
    private readonly object _lock = new();
    private ProfileSettings _settings = new();

    public ProfileSettings Settings
    {
        get
        {
            lock (_lock)
                return _settings;
        }
    }

    public void Update(ProfileSettings settings)
    {
        lock (_lock)
            _settings = settings;
    }
}
