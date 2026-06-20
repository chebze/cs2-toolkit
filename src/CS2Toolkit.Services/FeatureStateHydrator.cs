using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

internal sealed class FeatureStateHydrator : IHostedService
{
    private readonly IFeatureState _featureState;
    private readonly IConfigurationStore _configurationStore;
    private readonly IConfigurationChangeNotifier _changeNotifier;
    private readonly ILogger<FeatureStateHydrator> _logger;
    private string? _lastHydratedProfileId;

    public FeatureStateHydrator(
        IFeatureState featureState,
        IConfigurationStore configurationStore,
        IConfigurationChangeNotifier changeNotifier,
        ILogger<FeatureStateHydrator> logger)
    {
        _featureState = featureState;
        _configurationStore = configurationStore;
        _changeNotifier = changeNotifier;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _changeNotifier.ConfigurationChanged += OnConfigurationChanged;
        ApplyCurrentProfile();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _changeNotifier.ConfigurationChanged -= OnConfigurationChanged;
        return Task.CompletedTask;
    }

    private void OnConfigurationChanged() => ApplyCurrentProfile();

    private void ApplyCurrentProfile()
    {
        var profile = _configurationStore.GetActiveProfile();
        if (profile.Id == _lastHydratedProfileId)
            return;

        _lastHydratedProfileId = profile.Id;
        _featureState.ApplyFromProfile(profile.Settings);
        _logger.LogInformation(
            "Applied profile toggles for {ProfileName}",
            profile.Name);
    }
}
