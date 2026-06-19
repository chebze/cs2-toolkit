using Cs2Toolkit.Configuration;
using Cs2Toolkit.Tunnel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class ConfigWebState
{
    private readonly object _lock = new();
    private int _webPort;
    private bool _webReady;

    public event Action? WebReady;

    public bool IsWebReady
    {
        get
        {
            lock (_lock)
                return _webReady;
        }
    }

    public int WebPort
    {
        get
        {
            lock (_lock)
                return _webPort;
        }
    }

    public void SetWebReady(int port)
    {
        lock (_lock)
        {
            _webPort = port;
            _webReady = true;
        }

        WebReady?.Invoke();
    }
}

public sealed class LocalTunnelHostedService : IHostedService, IAsyncDisposable
{
    private readonly ConfigManager _configManager;
    private readonly ConfigWebState _webState;
    private readonly LocalTunnelState _tunnelState;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LocalTunnelHostedService> _logger;
    private readonly ILogger<LocalTunnelClient> _clientLogger;
    private LocalTunnelClient? _client;
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    public LocalTunnelHostedService(
        ConfigManager configManager,
        ConfigWebState webState,
        LocalTunnelState tunnelState,
        IHttpClientFactory httpClientFactory,
        ILogger<LocalTunnelHostedService> logger,
        ILogger<LocalTunnelClient> clientLogger)
    {
        _configManager = configManager;
        _webState = webState;
        _tunnelState = tunnelState;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _clientLogger = clientLogger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _webState.WebReady += OnWebReady;
        _configManager.StoreChanged += OnStoreChanged;

        if (_webState.IsWebReady)
            StartTunnelIfEnabled();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _webState.WebReady -= OnWebReady;
        _configManager.StoreChanged -= OnStoreChanged;

        if (_cts is not null)
            await _cts.CancelAsync();

        if (_runTask is not null)
        {
            try
            {
                await _runTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        await DisposeClientAsync();
        _tunnelState.SetDisabled();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
        _cts?.Dispose();
    }

    private void OnWebReady() => StartTunnelIfEnabled();

    private void OnStoreChanged() => RestartTunnel();

    private void RestartTunnel()
    {
        if (_cts?.IsCancellationRequested == true)
            return;

        _ = Task.Run(async () =>
        {
            await DisposeClientAsync();
            StartTunnelIfEnabled();
        });
    }

    private void StartTunnelIfEnabled()
    {
        if (_cts?.IsCancellationRequested == true || !_webState.IsWebReady)
            return;

        var store = _configManager.GetStore();
        if (!store.PublicTunnelEnabled)
        {
            _tunnelState.SetDisabled();
            return;
        }

        _runTask = RunTunnelAsync(_cts!.Token);
    }

    private async Task RunTunnelAsync(CancellationToken cancellationToken)
    {
        _tunnelState.SetConnecting();

        try
        {
            await DisposeClientAsync();

            var store = _configManager.GetStore();
            var httpClient = _httpClientFactory.CreateClient(nameof(LocalTunnelClient));
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            _client = new LocalTunnelClient(httpClient, _clientLogger);

            var options = new LocalTunnelOptions
            {
                Server = new Uri(store.PublicTunnelServer),
                Subdomain = store.PublicTunnelSubdomain,
                LocalHost = "127.0.0.1",
                LocalPort = _webState.WebPort,
                MaxConnections = store.PublicTunnelMaxConnections
            };

            var registration = await _client.ConnectAsync(options, cancellationToken);
            _tunnelState.SetConnected(registration.Url);
            _logger.LogInformation("Public config URL: {Url}", registration.Url);

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LocalTunnel failed");
            _tunnelState.SetFailed(ex.Message);
        }
    }

    private async Task DisposeClientAsync()
    {
        if (_client is null)
            return;

        await _client.DisposeAsync();
        _client = null;
    }
}
