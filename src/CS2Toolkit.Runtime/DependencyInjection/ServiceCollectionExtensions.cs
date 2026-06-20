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

internal sealed class StartupLoggerHostedService(ILogger<StartupLoggerHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CS2 Toolkit v2 ready");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
