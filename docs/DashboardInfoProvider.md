# DashboardInfoProvider

## Purpose

Default implementation of `IDashboardInfoProvider` for `GET /api/dashboard`.

## Key API

| Method | Description |
|--------|-------------|
| `GetDashboardInfo()` | Builds `DashboardInfo` from the active profile and configuration store |

## Behavior

- Uses `NetworkAccess.GetAccessUrls` with the store's `WebPort`.
- Returns `/radar` as the radar UI route.

## Dependencies

- `IConfigurationStore`

## Configuration

None.
