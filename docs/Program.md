# Program

## Purpose

Application entry point. Builds the generic host, registers all toolkit services as singletons, and starts the event-loop driven runtime.

## Startup sequence

1. Initializes WinForms (`ApplicationConfiguration.Initialize()`).
2. Creates `IHost` via `Host.CreateDefaultBuilder`.
3. Registers configuration, singletons, and hosted services.
4. Runs the host until shutdown.

## Dependency injection

| Service | Lifetime | Role |
|---------|----------|------|
| `ToolkitEventBus` | Singleton | Central event publisher |
| `RuntimeGate` | Singleton | Startup synchronization |
| `ProcessMemory` | Singleton | External process memory access |
| `ScreenOverlayManager` | Singleton | Full-screen overlay window |
| `OffsetDownloader` | Singleton | Downloads CS2 offsets |
| `FileLogWriter` | Singleton | Match log file writer |
| `EnemyOverlay` | Singleton + Hosted | Enemy stat rendering |
| `TeammateOverlay` | Singleton + Hosted | Teammate stat rendering |
| `BombOverlay` | Singleton + Hosted | Bomb carrier state rendering |
| `ClairvoyanceOverlay` | Singleton + Hosted | Contextual advisor tips |
| `MenuOverlay` | Singleton + Hosted | Settings menu |
| `ToolkitRuntime` | Hosted | Orchestrates startup and input |
| `GameMemoryReader` | Hosted | Polls game memory every 100ms |
| `MatchLogger` | Hosted | Round/file diagnostics |

## Fatal errors

If offset download or injection fails, `ToolkitRuntime` logs a critical error, sets `Environment.ExitCode = 1`, and stops the host. `Program` prints the message to stderr and pauses with "Press any key to exit..." when running interactively, so startup failures are visible instead of silently closing.

## Configuration

Reads `appsettings.json` from the `Toolkit` section. See [ToolkitOptions.md](ToolkitOptions.md).
