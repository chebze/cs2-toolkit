# LocalTunnelClient

## Purpose

Modern .NET implementation of a [localtunnel.me](https://localtunnel.me) client. Registers a public HTTPS URL that proxies HTTP traffic to a local port (the config UI).

## Key API

- `RegisterAsync(options)` — `GET /?new` or `GET /{subdomain}` on the tunnel server
- `StartRelayAsync(registration, options)` — opens pooled upstream TCP connections and bidirectionally proxies to the local host
- `ConnectAsync(options)` — register + start relay
- `StopRelayAsync()` / `DisposeAsync()` — tear down connections

## Behavior

1. Requests tunnel registration from the public LocalTunnel server
2. Connects to the server-assigned upstream TCP port (`registration.Port`)
3. Maintains multiple parallel upstream sockets (`max_conn_count`)
4. Rewrites the HTTP `Host` header to `127.0.0.1:{port}` so Kestrel accepts proxied requests
5. Automatically reconnects upstream sockets when they drop

## Dependencies

- `HttpClient` (injected via `IHttpClientFactory` in `LocalTunnelHostedService`)
- `ILogger<LocalTunnelClient>`

## Configuration

Stored in `ConfigStore`:

- `PublicTunnelEnabled` (default `true`)
- `PublicTunnelServer` (default `https://localtunnel.me`)
- `PublicTunnelSubdomain` (optional)
- `PublicTunnelMaxConnections` (default `4`)

## Related

- `LocalTunnelHostedService` — starts tunnel after config web host is ready
- `LocalTunnelState` — exposes public URL and status to the dashboard API
- `ConfigWebState` — signals when the local web port is listening
