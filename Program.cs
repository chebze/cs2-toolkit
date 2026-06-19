using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Logging;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Offsets;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Runtime;
using Cs2Toolkit.Services;
using Cs2Toolkit.Tunnel;
using Cs2Toolkit.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<ToolkitOptions>(context.Configuration.GetSection(ToolkitOptions.SectionName));

                services.AddSingleton<ConfigManager>();
                services.AddSingleton<RuntimeConfigProvider>();
                services.AddSingleton<OverlayStyleState>();
                services.AddSingleton<GlobalKeybindState>();
                services.AddSingleton<ConfigWebState>();
                services.AddSingleton<LocalTunnelState>();
                services.AddSingleton<ActiveWeaponTracker>();
                services.AddSingleton<WeaponConfigState>();

                services.AddSingleton<RadarTracker>();
                services.AddSingleton<RadarState>();

                services.AddSingleton<ToolkitEventBus>();
                services.AddSingleton<RuntimeGate>();
                services.AddSingleton<ProcessMemory>();
                services.AddSingleton<ScreenOverlayManager>();
                services.AddHttpClient();
                services.AddSingleton<OffsetDownloader>();

                services.AddSingleton<MapPhysicsParser>();
                services.AddSingleton<MapVisibilityChecker>();
                services.AddSingleton<MapDataService>();
                services.AddSingleton<MapNameReader>();

                services.AddSingleton<FileLogWriter>();
                services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
                services.AddSingleton<MatchLogger>();

                services.AddSingleton<EnemySoundTracker>();
                services.AddSingleton<EnemyLastSeenTracker>();
                services.AddSingleton<EnemyEspState>();
                services.AddSingleton<ViewMatrixHolder>();
                services.AddSingleton<EnemyOverlay>();
                services.AddSingleton<TeammateOverlay>();
                services.AddSingleton<BombOverlay>();
                services.AddSingleton<ClairvoyanceOverlay>();
                services.AddSingleton<EnemyNoiseOverlay>();
                services.AddSingleton<MenuOverlay>();

                services.AddSingleton<RcsState>();
                services.AddSingleton<RecoilCompensator>();
                services.AddSingleton<RcsOverlay>();
                services.AddSingleton<RcsToggleService>();

                services.AddSingleton<EnemyEspToggleService>();
                services.AddSingleton<EnemyEspStatusOverlay>();

                services.AddSingleton<SoundEspState>();
                services.AddSingleton<SoundEspToggleService>();
                services.AddSingleton<SoundEspStatusOverlay>();

                services.AddSingleton<AimHelperState>();
                services.AddSingleton<AimHelper>();
                services.AddSingleton<AimHelperToggleService>();
                services.AddSingleton<AimHelperOverlay>();

                services.AddSingleton<SettingsSaveService>();

                services.AddSingleton<TbState>();
                services.AddSingleton<Triggerbot>();
                services.AddSingleton<TbOverlay>();
                services.AddSingleton<TbToggleService>();
                services.AddSingleton<GrenadeTrajectoryTracker>();
                services.AddSingleton<GrenadeOverlay>();

                services.AddHostedService<ConfigWebHostService>();
                services.AddHostedService<LocalTunnelHostedService>();
                services.AddHostedService<LiveConfigApplier>();
                services.AddHostedService<ConfigProfileSwitchService>();
                services.AddHostedService<ToolkitRuntime>();
                services.AddHostedService<GameMemoryReader>();
                services.AddHostedService(sp => sp.GetRequiredService<MatchLogger>());
                services.AddHostedService(sp => sp.GetRequiredService<EnemyOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<TeammateOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<BombOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<ClairvoyanceOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<EnemyNoiseOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<MenuOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<RcsOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<RcsToggleService>());
                services.AddHostedService(sp => sp.GetRequiredService<EnemyEspToggleService>());
                services.AddHostedService(sp => sp.GetRequiredService<EnemyEspStatusOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<SoundEspToggleService>());
                services.AddHostedService(sp => sp.GetRequiredService<SoundEspStatusOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<AimHelperToggleService>());
                services.AddHostedService(sp => sp.GetRequiredService<AimHelperOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<SettingsSaveService>());
                services.AddHostedService(sp => sp.GetRequiredService<TbOverlay>());
                services.AddHostedService(sp => sp.GetRequiredService<TbToggleService>());
                services.AddHostedService(sp => sp.GetRequiredService<GrenadeOverlay>());
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        try
        {
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex}");
            Environment.ExitCode = 1;
        }

        if (Environment.ExitCode != 0)
        {
            Console.Error.WriteLine("The toolkit stopped due to a startup error. See logs above.");
            if (Environment.UserInteractive && !Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(intercept: true);
            }
        }
    }
}
