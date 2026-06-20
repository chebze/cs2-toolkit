using System.Diagnostics;
using CS2Toolkit.API;
using CS2Toolkit.API.Abstractions;
using CS2Toolkit.API.Endpoints;
using CS2Toolkit.API.StaticFiles;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Runtime.Web;

public sealed class ApiHostService : IHostedService
{
    private readonly IServiceProvider _rootServices;
    private readonly IConfigurationStore _configurationStore;
    private readonly IHostEnvironment _environment;
    private readonly ToolkitHostSettings _options;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly ILogger<ApiHostService> _logger;
    private WebApplication? _app;
    private CancellationTokenSource? _cts;

    public ApiHostService(
        IServiceProvider rootServices,
        IConfigurationStore configurationStore,
        IHostEnvironment environment,
        IOptions<ToolkitHostSettings> options,
        IRuntimeOrchestrator orchestrator,
        ILogger<ApiHostService> logger)
    {
        _rootServices = rootServices;
        _configurationStore = configurationStore;
        _environment = environment;
        _options = options.Value;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.WaitForPhaseAsync(StartupPhase.Maps, cancellationToken);
        var store = _configurationStore.GetStore();
        var requestedPort = store.WebPort;
        if (!NetworkAccess.TryFindAvailablePort(requestedPort, out var port))
        {
            _logger.LogCritical(
                "No available TCP port in range {StartPort}–{EndPort}",
                requestedPort,
                requestedPort + 99);
            throw new InvalidOperationException(
                $"No available TCP port in range {requestedPort}–{requestedPort + 99}.");
        }

        if (port != requestedPort)
        {
            _configurationStore.UpdateWebPort(port);
            _logger.LogWarning("Port {Requested} unavailable, using {Port}", requestedPort, port);
        }

        Environment.SetEnvironmentVariable(
            "ASPNETCORE_URLS",
            $"http://{(_options.BindApiToLocalhostOnly ? "127.0.0.1" : "0.0.0.0")}:{port}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = _environment.ContentRootPath
        });

        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(_rootServices.GetRequiredService<IConfigurationStore>());
        builder.Services.AddSingleton(_rootServices.GetRequiredService<IDashboardInfoProvider>());
        builder.Services.AddSingleton(_rootServices.GetRequiredService<IRadarStreamSource>());

        _app = builder.Build();
        var wwwroot = ToolkitStaticFileExtensions.ResolveWwwRootPath(_environment.ContentRootPath);
        _app.MapToolkitApi();
        _app.MapToolkitStaticFiles(wwwroot, _logger);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = _app.RunAsync(_cts.Token);

        var urls = NetworkAccess.GetAccessUrls(port, _options.BindApiToLocalhostOnly);
        _logger.LogInformation("Config UI available at {Urls}", string.Join(", ", urls));

        if (_options.OpenConfigUiOnStart)
            TryOpenBrowser(urls.FirstOrDefault() ?? $"http://localhost:{port}");

        _orchestrator.CompletePhase(StartupPhase.Api);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
            await _cts.CancelAsync();

        if (_app is not null)
            await _app.StopAsync(cancellationToken);
    }

    private static void TryOpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Browser launch is best-effort.
        }
    }
}
