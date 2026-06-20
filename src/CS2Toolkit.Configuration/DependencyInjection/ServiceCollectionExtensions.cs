using CS2Toolkit.Configuration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitConfiguration(this IServiceCollection services)
    {
        services.TryAddSingleton<LegacySettingsMigrator>();
        services.TryAddSingleton<JsonConfigurationStore>();
        services.TryAddSingleton<IConfigurationStore>(sp => sp.GetRequiredService<JsonConfigurationStore>());
        services.TryAddSingleton<IConfigurationChangeNotifier>(sp => sp.GetRequiredService<JsonConfigurationStore>());
        services.TryAddSingleton<ISettingsResolver, SettingsResolver>();
        services.TryAddSingleton<ActiveConfiguration>();
        services.TryAddSingleton<IActiveConfiguration>(sp => sp.GetRequiredService<ActiveConfiguration>());
        services.TryAddSingleton<IKeybindConfiguration>(sp => sp.GetRequiredService<ActiveConfiguration>());
        return services;
    }
}
