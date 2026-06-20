using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

internal sealed class FeatureStateHydrator : IHostedService
{
    private readonly IFeatureState _featureState;
    private readonly IActiveConfiguration _configuration;
    private readonly IConfigurationChangeNotifier _changeNotifier;
    private readonly ILogger<FeatureStateHydrator> _logger;

    public FeatureStateHydrator(
        IFeatureState featureState,
        IActiveConfiguration configuration,
        IConfigurationChangeNotifier changeNotifier,
        ILogger<FeatureStateHydrator> logger)
    {
        _featureState = featureState;
        _configuration = configuration;
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
        var profile = _configuration.Current.Profile;
        _featureState.ApplyFromProfile(profile);
        _logger.LogInformation(
            "Applied profile toggles for {ProfileName}",
            _configuration.Current.ActiveProfileName);
    }
}
