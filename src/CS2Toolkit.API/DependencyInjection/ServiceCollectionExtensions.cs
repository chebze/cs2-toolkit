using CS2Toolkit.API.Abstractions;
using CS2Toolkit.API.Dashboard;
using CS2Toolkit.API.Radar;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.API;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitApi(this IServiceCollection services)
    {
        services.TryAddSingleton<IDashboardInfoProvider, DashboardInfoProvider>();
        services.TryAddSingleton<IRadarStreamSource>(sp =>
            new RadarStreamSource(sp.GetRequiredService<IRadarSnapshotProvider>()));
        return services;
    }
}
