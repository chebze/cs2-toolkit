using CS2Toolkit.Drawing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Drawing.WinForms;

public static class DependencyInjection
{
    public static IServiceCollection AddDrawingWinForms(this IServiceCollection services)
    {
        services.TryAddSingleton<IWorldProjector, WorldProjector>();
        services.TryAddSingleton<LatestFrameOverlaySink>();
        services.TryAddSingleton<IOverlayFrameSink>(sp => sp.GetRequiredService<LatestFrameOverlaySink>());
        services.TryAddSingleton<IOverlayFrameSource>(sp => sp.GetRequiredService<LatestFrameOverlaySink>());

        services.TryAddSingleton<IOverlayViewport, WinFormsOverlayViewport>();
        services.TryAddSingleton<WinFormsOverlayRenderer>();
        services.TryAddSingleton<IOverlayRenderer>(sp => sp.GetRequiredService<WinFormsOverlayRenderer>());
        services.AddHostedService(sp => sp.GetRequiredService<WinFormsOverlayRenderer>());

        return services;
    }
}
