using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Tunnel;

internal static partial class HostHeaderRewriter
{
    [GeneratedRegex(@"(?i)(\r\nHost:\s*)\S+")]
    private static partial Regex HostHeaderPattern();

    public static ReadOnlyMemory<byte> RewriteFirstChunk(ReadOnlySpan<byte> data, string hostHeader)
    {
        if (!LooksLikeHttpRequest(data))
            return data.ToArray();

        var text = Encoding.ASCII.GetString(data);
        if (!HostHeaderPattern().IsMatch(text))
            return data.ToArray();

        var rewritten = HostHeaderPattern().Replace(text, $"$1{hostHeader}", 1);
        return Encoding.ASCII.GetBytes(rewritten);
    }

    private static bool LooksLikeHttpRequest(ReadOnlySpan<byte> data)
    {
        return data.Length >= 4
            && (data[0] is (byte)'G' or (byte)'P' or (byte)'H' or (byte)'D' or (byte)'C' or (byte)'O' or (byte)'T');
    }
}

internal static class StreamRelay
{
    public static async Task RelayAsync(
        Stream source,
        Stream destination,
        bool rewriteHostHeader,
        string? hostHeader,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        try
        {
            var firstChunk = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await source.ReadAsync(buffer.AsMemory(), cancellationToken);
                if (bytesRead == 0)
                    break;

                if (rewriteHostHeader && firstChunk && hostHeader is not null)
                {
                    firstChunk = false;
                    var rewritten = HostHeaderRewriter.RewriteFirstChunk(buffer.AsSpan(0, bytesRead), hostHeader);
                    await destination.WriteAsync(rewritten, cancellationToken);
                    continue;
                }

                firstChunk = false;
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

internal sealed class TunnelUpstreamConnection : IAsyncDisposable
{
    private readonly Func<CancellationToken, Task<Socket>> _connectUpstreamAsync;
    private readonly string _localHost;
    private readonly int _localPort;
    private readonly bool _rewriteHostHeader;
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly CancellationTokenSource _cts = new();
    private Task? _runTask;

    public TunnelUpstreamConnection(
        string name,
        Func<CancellationToken, Task<Socket>> connectUpstreamAsync,
        string localHost,
        int localPort,
        bool rewriteHostHeader,
        ILogger logger)
    {
        _name = name;
        _connectUpstreamAsync = connectUpstreamAsync;
        _localHost = localHost;
        _localPort = localPort;
        _rewriteHostHeader = rewriteHostHeader;
        _logger = logger;
    }

    public void Start() => _runTask = RunAsync(_cts.Token);

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var upstream = await _connectUpstreamAsync(cancellationToken);
                using var local = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await local.ConnectAsync(_localHost, _localPort, cancellationToken);

                await using var upstreamStream = new NetworkStream(upstream, ownsSocket: true);
                await using var localStream = new NetworkStream(local, ownsSocket: true);

                var hostHeader = $"{_localHost}:{_localPort}";
                var upstreamToLocal = StreamRelay.RelayAsync(
                    upstreamStream,
                    localStream,
                    _rewriteHostHeader,
                    hostHeader,
                    cancellationToken);
                var localToUpstream = StreamRelay.RelayAsync(
                    localStream,
                    upstreamStream,
                    rewriteHostHeader: false,
                    hostHeader: null,
                    cancellationToken);

                await Task.WhenAny(upstreamToLocal, localToUpstream);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "LocalTunnel upstream {Name} disconnected, reconnecting", _name);
                try
                {
                    await Task.Delay(500, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        if (_runTask is not null)
        {
            try
            {
                await _runTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _cts.Dispose();
    }
}
