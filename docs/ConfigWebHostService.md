# ConfigWebHostService

## Purpose

Hosts the React configuration UI and REST API on Kestrel at `http://0.0.0.0:{port}` (default 8080, increments until an available port is found). Opens the config URL in the default browser on startup.

## Key API

REST endpoints:

- `GET /api/dashboard` — active profile, access URLs, port
- `GET/POST/PUT/DELETE /api/configs` — profile CRUD
- `POST /api/configs/{id}/activate` / `default`
- `GET /api/configs/{id}/export`, `POST /api/configs/import`
- `GET/PUT /api/keybinds`
- `GET /api/weapons`

Static files served from `wwwroot/` (built from `ConfigUI/` during `dotnet build`).

## Behavior

- Runs as `IHostedService` alongside the toolkit host
- Listens on all interfaces for phone/LAN access
- SPA fallback to `index.html`

## Dependencies

- `ConfigManager`, `RuntimeConfigProvider`
- `IHostEnvironment`

## Configuration

- Port stored in `ConfigStore.WebPort`
