using CS2Toolkit.Services.Abstractions;
using CS2Toolkit.Services.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitServices(this IServiceCollection services)
    {
        services.TryAddSingleton<EnemyEspTracker>();
        services.TryAddSingleton<SoundEspWaveTracker>();
        services.TryAddSingleton<TriggerbotController>();
        services.TryAddSingleton<RcsController>();
        services.TryAddSingleton<AimHelperController>();
        services.TryAddSingleton<RadarState>();
        services.TryAddSingleton<IRadarSnapshotProvider>(sp => sp.GetRequiredService<RadarState>());
        services.TryAddSingleton<StatusToastStore>();
        services.TryAddSingleton<IStatusToastPublisher>(sp => sp.GetRequiredService<StatusToastStore>());
        services.TryAddSingleton<IFeatureState, FeatureRuntimeState>();
        services.TryAddSingleton<ProfileSettingsSaver>();
        services.TryAddSingleton<FeatureRegistry>();
        services.TryAddSingleton<IFeatureRegistry>(sp => sp.GetRequiredService<FeatureRegistry>());
        services.AddHostedService(sp => sp.GetRequiredService<FeatureRegistry>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, RcsFeatureService>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, TriggerbotFeatureService>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, EnemyEspFeatureService>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, SoundEspFeatureService>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, AimHelperFeatureService>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, MenuFeatureService>());

        services.TryAddSingleton<IOverlayComposer, OverlayComposer>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, DebugPlayerBoxPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, FeatureStatusOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, TeammateStatsOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, BombStatusOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, ClairvoyanceOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, EnemyEspOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, SoundEspOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, GrenadeArcOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, TriggerbotOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, RcsOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, AimHelperOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, MenuOverlayPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, StatusToastOverlayPresenter>());

        services.AddHostedService<FeatureCoordinator>();
        services.AddHostedService<RadarStateUpdater>();
        services.AddHostedService<StatusToastOrchestrator>();

        return services;
    }
}
