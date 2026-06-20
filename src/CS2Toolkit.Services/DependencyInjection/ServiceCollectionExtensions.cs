using CS2Toolkit.Services.Abstractions;
using CS2Toolkit.Services.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeatureState, FeatureRuntimeState>();
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

        services.AddHostedService<FeatureCoordinator>();

        return services;
    }
}
