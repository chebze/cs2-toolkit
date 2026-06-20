using CS2Toolkit.Runtime.Abstractions;
using CS2Toolkit.Runtime.Orchestration;
using CS2Toolkit.Runtime.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CS2Toolkit.Runtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRuntimeOrchestration(this IServiceCollection services)
    {
        services.TryAddSingleton<IRuntimeOrchestrator, RuntimeOrchestrator>();
        services.AddHostedService<RuntimeOrchestratorHostedService>();
        services.AddHostedService<StartupLoggerHostedService>();
        services.AddHostedService<InjectKeybindOrchestrator>();
        services.AddHostedService<ApiHostService>();
        return services;
    }
}
