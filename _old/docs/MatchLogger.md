# MatchLogger

## Purpose

Hosted service that subscribes to `OnMemoryRead` and `OnInjectionStatusChanged`, writing structured per-round diagnostics to `FileLogWriter`.

## Round detection

Tracks `RoundInfo` from `MemoryState` and detects transitions:

| Event | Trigger |
|-------|---------|
| `ROUND_START` | `RoundStartCount` changed |
| `ROUND_END` | `RoundEndCount` changed |
| `MATCH_ENTER` | `IsInMatch` became `true` |
| `MATCH_EXIT` | `IsInMatch` became `false` |

On first in-match read, logs `ROUND_TRACKING` with initial counter values.

## Logged data

### Round start/end

- Round counters (`startCount`, `endCount`, `totalPlayed`)
- Freeze/warmup/phase flags
- Current enemy/teammate alive/dead stats
- Round end includes winner team and `winStatus`

### Stat changes

When any alive/dead count changes, logs a `STATS` line with current totals and freeze state.

### Player details

On round start/end (if enabled), logs each player: index, role, team, name, alive, health.

## Dependencies

- `ToolkitEventBus`
- `FileLogWriter`
- `IOptions<ToolkitOptions>`

## Registration

Singleton + `IHostedService` in DI.
