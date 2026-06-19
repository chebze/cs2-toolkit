namespace Cs2Toolkit.Tunnel;

public sealed class LocalTunnelOptions
{
    public Uri Server { get; set; } = new("https://localtunnel.me");

    public string? Subdomain { get; set; }

    public string LocalHost { get; set; } = "127.0.0.1";

    public int LocalPort { get; set; } = 8080;

    public int MaxConnections { get; set; } = 10;

    public bool RewriteHostHeader { get; set; } = true;

    public TimeSpan RegistrationRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}

public enum LocalTunnelStatus
{
    Disabled,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}

public sealed class LocalTunnelState
{
    private readonly object _lock = new();
    private LocalTunnelStatus _status = LocalTunnelStatus.Disabled;
    private string? _publicUrl;
    private string? _error;

    public event Action? Changed;

    public LocalTunnelStatus Status
    {
        get
        {
            lock (_lock)
                return _status;
        }
    }

    public string? PublicUrl
    {
        get
        {
            lock (_lock)
                return _publicUrl;
        }
    }

    public string? Error
    {
        get
        {
            lock (_lock)
                return _error;
        }
    }

    public void SetConnecting()
    {
        lock (_lock)
        {
            _status = LocalTunnelStatus.Connecting;
            _publicUrl = null;
            _error = null;
        }

        Changed?.Invoke();
    }

    public void SetConnected(string publicUrl)
    {
        lock (_lock)
        {
            _status = LocalTunnelStatus.Connected;
            _publicUrl = publicUrl;
            _error = null;
        }

        Changed?.Invoke();
    }

    public void SetReconnecting()
    {
        lock (_lock)
        {
            if (_status is LocalTunnelStatus.Disabled or LocalTunnelStatus.Failed)
                return;

            _status = LocalTunnelStatus.Reconnecting;
        }

        Changed?.Invoke();
    }

    public void SetFailed(string error)
    {
        lock (_lock)
        {
            _status = LocalTunnelStatus.Failed;
            _error = error;
        }

        Changed?.Invoke();
    }

    public void SetDisabled()
    {
        lock (_lock)
        {
            _status = LocalTunnelStatus.Disabled;
            _publicUrl = null;
            _error = null;
        }

        Changed?.Invoke();
    }
}
