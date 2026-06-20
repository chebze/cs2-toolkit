using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IOverlayComposer, OverlayComposer>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOverlayPresenter, DebugPlayerBoxPresenter>());
        services.AddHostedService<OverlayPipelineHostedService>();

        return services;
    }
}
