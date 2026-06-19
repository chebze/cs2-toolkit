# LocalTunnelHostedService

## Purpose

Hosted service that opens a public LocalTunnel URL for the configuration web UI once Kestrel is listening.

## Key API

- `IHostedService.StartAsync` — waits for `ConfigWebState.WebReady`, then starts `LocalTunnelClient`
- Restarts tunnel when `ConfigManager.StoreChanged` (e.g. tunnel settings updated)

## Behavior

- Disabled when `ConfigStore.PublicTunnelEnabled` is `false`
- Updates `LocalTunnelState` with `Connecting`, `Connected`, `Failed`, or `Disabled`
- Public URL is returned on `/api/dashboard` as `publicTunnel.url`

## Dependencies

- `ConfigManager`, `ConfigWebState`, `LocalTunnelState`
- `LocalTunnelClient`, `IHttpClientFactory`

## Configuration

See [LocalTunnelClient.md](LocalTunnelClient.md).
