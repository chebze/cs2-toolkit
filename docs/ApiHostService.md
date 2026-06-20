# ApiHostService

## Purpose

`IHostedService` that starts an in-process Kestrel web server for the configuration UI and REST API.

## Key API

Implements `IHostedService`:

| Method | Description |
|--------|-------------|
| `StartAsync` | Resolves an available port, updates the store if needed, builds `WebApplication`, maps toolkit API + static files, runs Kestrel |
| `StopAsync` | Cancels and stops the web application |

## Behavior

- Binds to `127.0.0.1:{port}` when `Toolkit:BindApiToLocalhostOnly` is `true` (default); otherwise `0.0.0.0:{port}`.
- If the requested port is taken, scans up to 100 ports and persists the chosen port via `IConfigurationStore.UpdateWebPort`.
- Forwards `IConfigurationStore`, `IDashboardInfoProvider`, and `IRadarStreamSource` from the root host DI container into the web app.
- Serves static files from `{AppContext.BaseDirectory}/wwwroot` when `index.html` exists (built from `CS2Toolkit.Frontend` during `dotnet build`).
- Opens the config UI in the default browser when `ToolkitHostSettings.OpenConfigUiOnStart` is true (default).

## Dependencies

- `IConfigurationStore`
- `IDashboardInfoProvider`
- `IRadarStreamSource`
- `IHostEnvironment`
- `IOptions<ToolkitHostSettings>`

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `Toolkit:OpenConfigUiOnStart` | `true` | Open browser on API host start |
| `Toolkit:BindApiToLocalhostOnly` | `true` | Bind Kestrel to loopback only |
| Store `webPort` | `8080` | Preferred HTTP port |
