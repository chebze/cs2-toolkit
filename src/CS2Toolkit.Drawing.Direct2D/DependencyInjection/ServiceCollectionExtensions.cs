using CS2Toolkit.Drawing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Drawing.Direct2D;

public static class DependencyInjection
{
    public static IServiceCollection AddDrawingDirect2D(this IServiceCollection services)
    {
        services.TryAddSingleton<IWorldProjector, WorldProjector>();
        services.TryAddSingleton<LatestFrameOverlaySink>();
        services.TryAddSingleton<IOverlayFrameSink>(sp => sp.GetRequiredService<LatestFrameOverlaySink>());
        services.TryAddSingleton<IOverlayFrameSource>(sp => sp.GetRequiredService<LatestFrameOverlaySink>());

        services.TryAddSingleton<IOverlayViewport, Direct2DOverlayViewport>();
        services.TryAddSingleton<Direct2DOverlayRenderer>();
        services.TryAddSingleton<IOverlayRenderer>(sp => sp.GetRequiredService<Direct2DOverlayRenderer>());
        services.AddHostedService(sp => sp.GetRequiredService<Direct2DOverlayRenderer>());

        return services;
    }
}
