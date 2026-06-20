# FileLogWriter

## Purpose

Thread-safe append-only writer that creates timestamped log files for match diagnostics.

## Configuration

`Toolkit:FileLogging` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Enable file output |
| `Directory` | `logs` | Output folder (created if missing) |
| `FileNamePrefix` | `cs2-toolkit` | Log file name prefix |
| `LogStatChanges` | `true` | Log when alive/dead counts change |
| `LogPlayerDetailsOnRoundEvents` | `true` | Dump all players on round start/end |

## File naming

`{Directory}/{FileNamePrefix}-{yyyy-MM-dd-HHmmss}.log`

## API

- `Write(category, message)` — writes `timestamp [category] message`
- `IsEnabled` — whether file logging is active
- `FilePath` — path to the current log file

## Categories

Written by `MatchLogger` and `FileLoggerProvider`:

| Category | Meaning |
|----------|---------|
| `SESSION` | Log file open/close |
| `INJECTION` | Inject flow status |
| `MATCH_ENTER` / `MATCH_EXIT` | Entered/left live match |
| `ROUND_TRACKING` | Initial round counter baseline |
| `ROUND_START` | New round detected |
| `ROUND_END` | Round end detected |
| `STATS` | Alive/dead count changed |
| `PLAYER` | Per-player snapshot |

## Registration

Singleton in DI. Disposed when the host shuts down.
