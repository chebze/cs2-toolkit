using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

internal sealed class ActiveProfileSwitcher : IActiveProfileSwitcher, IHostedService
{
    private readonly IConfigurationStore _configurationStore;
    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _featureState;
    private readonly IConfigurationChangeNotifier _changeNotifier;
    private readonly IProfileRuntimeSync _runtimeSync;
    private readonly ILogger<ActiveProfileSwitcher> _logger;
    private string? _lastAppliedProfileId;
    private bool _applying;

    public ActiveProfileSwitcher(
        IConfigurationStore configurationStore,
        IActiveConfiguration configuration,
        IFeatureState featureState,
        IConfigurationChangeNotifier changeNotifier,
        IProfileRuntimeSync runtimeSync,
        ILogger<ActiveProfileSwitcher> logger)
    {
        _configurationStore = configurationStore;
        _configuration = configuration;
        _featureState = featureState;
        _changeNotifier = changeNotifier;
        _runtimeSync = runtimeSync;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _changeNotifier.ConfigurationChanged += OnConfigurationChanged;
        TryApplyActiveProfile();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _changeNotifier.ConfigurationChanged -= OnConfigurationChanged;
        return Task.CompletedTask;
    }

    public void SwitchTo(string profileId)
    {
        using (_runtimeSync.Acquire())
        {
            _applying = true;
            try
            {
                _configurationStore.SetActiveProfile(profileId);
                ApplyActiveProfileCore();
            }
            finally
            {
                _applying = false;
            }
        }
    }

    public void ApplyActiveProfileToggles(ProfileSettings settings)
    {
        using (_runtimeSync.Acquire())
        {
            _configuration.Refresh();
            _featureState.ApplyFromProfile(settings);
        }
    }

    private void OnConfigurationChanged()
    {
        if (_applying)
            return;

        TryApplyActiveProfile();
    }

    private void TryApplyActiveProfile()
    {
        using (_runtimeSync.Acquire())
            ApplyActiveProfileCore();
    }

    private void ApplyActiveProfileCore()
    {
        var profile = _configurationStore.GetActiveProfile();
        if (profile.Id == _lastAppliedProfileId)
            return;

        _configuration.Refresh();
        _featureState.ApplyFromProfile(profile.Settings);
        _lastAppliedProfileId = profile.Id;
        _logger.LogInformation("Applied profile toggles for {ProfileName}", profile.Name);
    }
}
