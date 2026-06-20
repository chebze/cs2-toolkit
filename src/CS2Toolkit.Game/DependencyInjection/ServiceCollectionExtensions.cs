using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Game.Offsets;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Game.Services;
using CS2Toolkit.Models.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CS2Toolkit.Game;

public static class DependencyInjection
{
    public static IServiceCollection AddToolkitGame(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(OffsetDownloader));

        services.TryAddSingleton<ProcessMemory>();
        services.TryAddSingleton<OffsetDownloader>();
        services.TryAddSingleton<OffsetProviderService>();
        services.TryAddSingleton<IOffsetProvider>(sp => sp.GetRequiredService<OffsetProviderService>());

        services.TryAddSingleton<GameStatePublisher>();
        services.TryAddSingleton<IGameStateSource>(sp => sp.GetRequiredService<GameStatePublisher>());
        services.TryAddSingleton<IReadOnlyGameState>(sp => sp.GetRequiredService<GameStatePublisher>());

        services.TryAddSingleton<GameAttachmentService>();
        services.TryAddSingleton<IGameAttachment>(sp => sp.GetRequiredService<GameAttachmentService>());
        services.TryAddSingleton<IGameLifecycle>(sp => sp.GetRequiredService<GameAttachmentService>());

        services.TryAddSingleton<MapCatalogService>();
        services.TryAddSingleton<IMapCatalog>(sp => sp.GetRequiredService<MapCatalogService>());
        services.TryAddSingleton<MapVisibilityStub>();
        services.TryAddSingleton<IMapVisibility>(sp => sp.GetRequiredService<MapVisibilityStub>());

        services.AddHostedService<OffsetBootstrapHostedService>();
        services.AddHostedService<GameMemoryLoop>();

        return services;
    }
}
