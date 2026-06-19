using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Web;

public sealed class ConfigWebHostService : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly ConfigManager _configManager;
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly RadarState _radarState;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ConfigWebHostService> _logger;
    private WebApplication? _app;
    private CancellationTokenSource? _cts;

    public ConfigWebHostService(
        ConfigManager configManager,
        RuntimeConfigProvider runtimeConfig,
        RadarState radarState,
        IHostEnvironment environment,
        ILogger<ConfigWebHostService> logger)
    {
        _configManager = configManager;
        _runtimeConfig = runtimeConfig;
        _radarState = radarState;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var store = _configManager.GetStore();
        var port = FindAvailablePort(store.WebPort);
        if (port != store.WebPort)
        {
            _configManager.UpdateWebPort(port);
            _logger.LogWarning("Port {Requested} unavailable, using {Port}", store.WebPort, port);
        }

        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = _environment.ContentRootPath
        });

        builder.Logging.ClearProviders();

        _app = builder.Build();
        var wwwroot = ResolveWwwRootPath();
        MapApi(_app);
        MapRadar(_app);
        MapStaticFiles(_app, wwwroot);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = _app.RunAsync(_cts.Token);

        var urls = GetAccessUrls(port);
        _logger.LogInformation("Config UI available at {Urls}", string.Join(", ", urls));
        TryOpenBrowser(urls.FirstOrDefault() ?? $"http://localhost:{port}");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
            await _cts.CancelAsync();

        if (_app is not null)
            await _app.StopAsync(cancellationToken);
    }

    private void MapApi(WebApplication app)
    {
        app.MapGet("/api/dashboard", () =>
        {
            var store = _configManager.GetStore();
            var active = _runtimeConfig.ActiveProfile;
            return Results.Json(new
            {
                activeProfile = new { active.Id, active.Name, active.SwitchHotkey },
                defaultProfileId = store.DefaultProfileId,
                accessUrls = GetAccessUrls(store.WebPort),
                webPort = store.WebPort,
                radarUrl = $"/radar"
            }, JsonOptions);
        });

        app.MapGet("/api/configs", () => Results.Json(_configManager.GetStore(), JsonOptions));

        app.MapGet("/api/configs/{id}", (string id) =>
        {
            var profile = _configManager.GetProfile(id);
            return profile is null ? Results.NotFound() : Results.Json(profile, JsonOptions);
        });

        app.MapPost("/api/configs", (CreateProfileRequest request) =>
        {
            var profile = _configManager.CreateProfile(request.Name);
            return Results.Json(profile, JsonOptions);
        });

        app.MapPut("/api/configs/{id}", (string id, ConfigProfile profile) =>
        {
            if (id != profile.Id)
                return Results.BadRequest("Profile id mismatch.");

            var updated = _configManager.UpdateProfile(profile);
            return Results.Json(updated, JsonOptions);
        });

        app.MapDelete("/api/configs/{id}", (string id) =>
        {
            _configManager.DeleteProfile(id);
            return Results.NoContent();
        });

        app.MapPost("/api/configs/{id}/activate", (string id) =>
        {
            _configManager.SetActiveProfile(id);
            return Results.Ok();
        });

        app.MapPost("/api/configs/{id}/default", (string id) =>
        {
            _configManager.SetDefaultProfile(id);
            return Results.Ok();
        });

        app.MapGet("/api/configs/{id}/export", (string id) =>
        {
            try
            {
                return Results.Text(_configManager.ExportProfile(id), "application/json");
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        app.MapPost("/api/configs/import", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            var name = request.Query.TryGetValue("name", out var nameValues)
                ? nameValues.FirstOrDefault()
                : null;
            var profile = _configManager.ImportProfile(json, name);
            return Results.Json(profile, JsonOptions);
        });

        app.MapGet("/api/keybinds", () => Results.Json(_configManager.GetStore().Keybinds, JsonOptions));

        app.MapPut("/api/keybinds", (GlobalKeybinds keybinds) =>
        {
            _configManager.UpdateKeybinds(keybinds);
            return Results.Ok();
        });

        app.MapGet("/api/weapons", () => Results.Json(WeaponCatalog.All, JsonOptions));
    }

    private void MapRadar(WebApplication app)
    {
        app.MapGet("/api/radar/snapshot", () =>
            Results.Json(_radarState.GetSnapshot(), JsonOptions));

        app.MapGet("/api/radar/stream", async (HttpContext context, CancellationToken cancellationToken) =>
        {
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.ContentType = "text/event-stream";

            var lastVersion = -1L;
            while (!cancellationToken.IsCancellationRequested)
            {
                var version = _radarState.Version;
                if (version != lastVersion)
                {
                    lastVersion = version;
                    await context.Response.WriteAsync($"data: {_radarState.GetSnapshotJson()}\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }

                await Task.Delay(100, cancellationToken);
            }
        });
    }

    private void MapStaticFiles(WebApplication app, string wwwroot)
    {
        if (!Directory.Exists(wwwroot))
            Directory.CreateDirectory(wwwroot);

        if (!File.Exists(Path.Combine(wwwroot, "index.html")))
        {
            _logger.LogWarning(
                "Config UI assets missing at {Wwwroot}. Run dotnet build to generate wwwroot/index.html.",
                wwwroot);
        }

        var fileProvider = new PhysicalFileProvider(wwwroot);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });
    }

    private string ResolveWwwRootPath()
    {
        var outputWwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        if (File.Exists(Path.Combine(outputWwwroot, "index.html")))
            return outputWwwroot;

        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private static int FindAvailablePort(int startPort)
    {
        for (var port = startPort; port < startPort + 100; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }

        return startPort;
    }

    private static bool IsPortAvailable(int port)
    {
        if (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
            .Any(endpoint => endpoint.Port == port))
            return false;

        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private static IReadOnlyList<string> GetAccessUrls(int port)
    {
        var urls = new List<string> { $"http://localhost:{port}" };
        foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                urls.Add($"http://{address}:{port}");
        }

        return urls.Distinct().ToList();
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

    private sealed record CreateProfileRequest(string Name);
}
