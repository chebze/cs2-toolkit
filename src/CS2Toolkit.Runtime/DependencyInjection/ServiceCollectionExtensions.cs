using CS2Toolkit.Configuration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Runtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRuntimeOrchestration(this IServiceCollection services)
    {
        services.AddHostedService<StartupLoggerHostedService>();
        return services;
    }
}

internal sealed class StartupLoggerHostedService(
    ILogger<StartupLoggerHostedService> logger,
    IActiveConfiguration configuration) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CS2 Toolkit v2 ready");
        logger.LogInformation(
            "Active profile: {ProfileName} ({ProfileId}), web port {WebPort}",
            configuration.Current.ActiveProfileName,
            configuration.Current.ActiveProfileId,
            configuration.Current.WebPort);

        var ak = configuration.ResolveWeapon(7);
        logger.LogInformation(
            "Resolved AK-47 triggerbot enabled={Enabled}, preFireFov={Fov}",
            ak.Triggerbot.Enabled,
            ak.Triggerbot.PreFireFovDegrees);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
