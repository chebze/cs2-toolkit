using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Tunnel;

public sealed class LocalTunnelClient : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalTunnelClient> _logger;
    private readonly List<TunnelUpstreamConnection> _connections = [];
    private readonly CancellationTokenSource _disposeCts = new();
    private bool _disposed;

    public LocalTunnelClient(HttpClient httpClient, ILogger<LocalTunnelClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public LocalTunnelRegistration? ActiveRegistration { get; private set; }

    public async Task<LocalTunnelRegistration> RegisterAsync(
        LocalTunnelOptions options,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var baseUri = options.Server.ToString().TrimEnd('/');
        var requestUri = options.Subdomain is { Length: > 0 } subdomain
            ? $"{baseUri}/{subdomain}"
            : $"{baseUri}/?new";

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                response.EnsureSuccessStatusCode();

                var registration = await response.Content.ReadFromJsonAsync<LocalTunnelRegistration>(cancellationToken)
                    ?? throw new InvalidOperationException("LocalTunnel server returned an empty response.");

                if (string.IsNullOrWhiteSpace(registration.Url))
                    throw new InvalidOperationException("LocalTunnel server did not return a public URL.");

                ActiveRegistration = registration;
                _logger.LogInformation(
                    "LocalTunnel registered {Id} -> {Url} (upstream port {Port}, max {MaxConn})",
                    registration.Id,
                    registration.Url,
                    registration.Port,
                    registration.MaxConnectionCount);

                return registration;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "LocalTunnel registration failed, retrying in {Delay}", options.RegistrationRetryDelay);
                await Task.Delay(options.RegistrationRetryDelay, cancellationToken);
            }
        }

        throw new OperationCanceledException(cancellationToken);
    }

    public async Task StartRelayAsync(
        LocalTunnelRegistration registration,
        LocalTunnelOptions options,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await StopRelayAsync();

        var upstreamHost = registration.Ip ?? options.Server.Host;
        var upstreamPort = registration.Port;
        var connectionCount = Math.Clamp(
            Math.Min(options.MaxConnections, registration.MaxConnectionCount),
            1,
            10);

        async Task<Socket> ConnectUpstreamAsync(CancellationToken ct)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            if (IPAddress.TryParse(upstreamHost, out var ipAddress))
            {
                await socket.ConnectAsync(new IPEndPoint(ipAddress, upstreamPort), ct);
                return socket;
            }

            await socket.ConnectAsync(upstreamHost, upstreamPort, ct);
            return socket;
        }

        for (var i = 0; i < connectionCount; i++)
        {
            var connection = new TunnelUpstreamConnection(
                $"conn-{i}",
                ConnectUpstreamAsync,
                options.LocalHost,
                options.LocalPort,
                options.RewriteHostHeader,
                _logger);

            connection.Start();
            _connections.Add(connection);
        }

        _logger.LogInformation(
            "LocalTunnel relaying {Url} -> {Host}:{Port} with {Count} upstream connection(s)",
            registration.Url,
            options.LocalHost,
            options.LocalPort,
            connectionCount);

        await Task.CompletedTask;
    }

    public async Task<LocalTunnelRegistration> ConnectAsync(
        LocalTunnelOptions options,
        CancellationToken cancellationToken = default)
    {
        var registration = await RegisterAsync(options, cancellationToken);
        await StartRelayAsync(registration, options, cancellationToken);
        return registration;
    }

    public async Task StopRelayAsync()
    {
        if (_connections.Count == 0)
            return;

        var connections = _connections.ToArray();
        _connections.Clear();

        foreach (var connection in connections)
            await connection.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        await _disposeCts.CancelAsync();
        await StopRelayAsync();
        _disposeCts.Dispose();
    }
}
