# IDashboardInfoProvider

## Purpose

HTTP-facing contract for the dashboard summary returned by `GET /api/dashboard`.

## Key API

| Member | Description |
|--------|-------------|
| `GetDashboardInfo()` | Returns active profile, default profile id, LAN access URLs, web port, and radar route. |

## Behavior

- Reads live data from `IConfigurationStore` on each request.
- Access URLs are derived from the configured `WebPort` in the configuration store.

## Dependencies

- `IConfigurationStore` (implemented by `DashboardInfoProvider` in `CS2Toolkit.API`)

## Configuration

None.
