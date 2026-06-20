using CS2Toolkit.Input.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Input;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitInput(this IServiceCollection services)
    {
        services.TryAddSingleton<Win32InputSimulator>();
        services.TryAddSingleton<IInputSimulator>(sp => sp.GetRequiredService<Win32InputSimulator>());
        services.TryAddSingleton<Win32InputListener>();
        services.TryAddSingleton<IInputListener>(sp => sp.GetRequiredService<Win32InputListener>());
        services.TryAddSingleton<IInputState>(sp => sp.GetRequiredService<Win32InputListener>());
        services.TryAddSingleton<IKeybindMatcher, KeybindMatcher>();
        services.TryAddSingleton<KeybindDispatcher>();
        services.TryAddSingleton<IKeybindDispatcher>(sp => sp.GetRequiredService<KeybindDispatcher>());
        services.AddHostedService(sp => sp.GetRequiredService<Win32InputListener>());
        services.AddHostedService(sp => sp.GetRequiredService<KeybindDispatcher>());
        return services;
    }
}
