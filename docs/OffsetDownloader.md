# OffsetDownloader

## Purpose

Downloads and parses CS2 memory offsets from the [a2x/cs2-dumper](https://github.com/a2x/cs2-dumper) GitHub repository at startup.

## Downloaded files

| URL config key | Source | Contents |
|----------------|--------|----------|
| `OffsetsUrl` | `output/offsets.json` | Module RVAs (`dwEntityList`, `dwLocalPlayerPawn`, etc.) |
| `ClientDllUrl` | `output/client_dll.json` | Class field offsets (`m_iHealth`, `m_iTeamNum`, etc.) |

URLs are configured in `appsettings.json` under `Toolkit:Offsets`.

## Parsed offsets

Produces a `GameOffsets` instance with:

- `DwEntityList`
- `DwLocalPlayerPawn`
- `DwLocalPlayerController`
- `M_hPlayerPawn`
- `M_iTeamNum`
- `M_iHealth`
- `M_lifeState`
- `M_iszPlayerName` (resolved with class fallbacks; see below)

## Player name offset resolution

`m_iszPlayerName` is defined on `CBasePlayerController`, not `CCSPlayerController`. Resolution order:

1. `m_iszPlayerName` on `CBasePlayerController`
2. `m_iszPlayerName` on `CCSPlayerController`
3. `m_sSanitizedPlayerName` on `CCSPlayerController`
4. `nint.Zero` — stats still work; names fall back to `Player {index}`

## Failure behavior

Required offsets throw `InvalidOperationException` on missing keys. `ToolkitRuntime` catches fatal errors, sets exit code 1, and stops the host. `Program` prints the error and pauses the console when interactive.

## Caching

`Offsets` property holds the last successfully downloaded offsets. `GameMemoryReader` reads this after the gate opens.

## Dependencies

- `IHttpClientFactory` — HTTP client for downloads
- `IOptions<ToolkitOptions>` — URL configuration
