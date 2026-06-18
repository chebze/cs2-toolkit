# Architecture Overview

## Event-loop driven design

```
Program
  └── IHost (Generic Host + DI)
        ├── ToolkitRuntime        → offsets, injection, input loop
        ├── GameMemoryReader      → 100ms memory poll
        ├── EnemyOverlay          → OnMemoryRead → draw
        ├── TeammateOverlay       → OnMemoryRead → draw
        ├── BombOverlay           → OnMemoryRead → draw
        ├── ClairvoyanceOverlay   → OnMemoryRead → draw
        └── MenuOverlay           → OnKeyPress → draw
```

## Event bus

All cross-component communication flows through `ToolkitEventBus`:

```
ToolkitRuntime ──OnKey* / OnMouse*──► MenuOverlay
GameMemoryReader ──OnMemoryRead──► EnemyOverlay, TeammateOverlay, BombOverlay, ClairvoyanceOverlay
ToolkitRuntime ──OnInjectionStatus──► (future subscribers)
```

## Startup order

1. Offsets downloaded (fatal on failure)
2. Overlay window created
3. Injection flow (wait for CS2 + key press)
4. Memory reader starts (gated by `RuntimeGate`)
5. Overlays subscribe and render on each event

## Running

```bash
dotnet run
```

1. Start CS2.
2. Launch the toolkit.
3. Press **F9** (default) when prompted.
4. Stats appear on the overlay.
5. Press **Insert** (default) to open the settings menu.

## Project structure

```
Configuration/   ToolkitOptions
Events/          ToolkitEventBus, event args
Memory/          ProcessMemory, EntityResolver
Models/          MemoryState, PlayerInfo, GameOffsets
Offsets/         OffsetDownloader
Overlay/         ScreenOverlayManager, OverlayLayer
Runtime/         RuntimeGate
Services/        ToolkitRuntime, GameMemoryReader, overlays
Utilities/       DrawHelper, KeyParser, NativeInput
docs/            Per-class documentation
```

## Class documentation index

| Class | Doc |
|-------|-----|
| Program | [Program.md](Program.md) |
| ToolkitRuntime | [ToolkitRuntime.md](ToolkitRuntime.md) |
| ScreenOverlayManager | [ScreenOverlayManager.md](ScreenOverlayManager.md) |
| OverlayLayer | [OverlayLayer.md](OverlayLayer.md) |
| GameMemoryReader | [GameMemoryReader.md](GameMemoryReader.md) |
| EnemyOverlay | [EnemyOverlay.md](EnemyOverlay.md) |
| EnemyLastSeenTracker | [EnemyLastSeenTracker.md](EnemyLastSeenTracker.md) |
| TeammateOverlay | [TeammateOverlay.md](TeammateOverlay.md) |
| BombOverlay | [BombOverlay.md](BombOverlay.md) |
| ClairvoyanceAdvisor | [ClairvoyanceAdvisor.md](ClairvoyanceAdvisor.md) |
| ClairvoyanceOverlay | [ClairvoyanceOverlay.md](ClairvoyanceOverlay.md) |
| BombInfo | [BombInfo.md](BombInfo.md) |
| BombStatus | [BombStatus.md](BombStatus.md) |
| MenuOverlay | [MenuOverlay.md](MenuOverlay.md) |
| ToolkitEventBus | [ToolkitEventBus.md](ToolkitEventBus.md) |
| RuntimeGate | [RuntimeGate.md](RuntimeGate.md) |
| OffsetDownloader | [OffsetDownloader.md](OffsetDownloader.md) |
| ProcessMemory | [ProcessMemory.md](ProcessMemory.md) |
| EntityResolver | [EntityResolver.md](EntityResolver.md) |
| MemoryState | [MemoryState.md](MemoryState.md) |
| PlayerInfo | [PlayerInfo.md](PlayerInfo.md) |
| GameOffsets | [GameOffsets.md](GameOffsets.md) |
| ToolkitOptions | [ToolkitOptions.md](ToolkitOptions.md) |
| ToolkitEvents | [ToolkitEvents.md](ToolkitEvents.md) |
| FileLogWriter | [FileLogWriter.md](FileLogWriter.md) |
| FileLoggerProvider | [FileLoggerProvider.md](FileLoggerProvider.md) |
| MatchLogger | [MatchLogger.md](MatchLogger.md) |
| RoundInfo | [RoundInfo.md](RoundInfo.md) |
| GameWindowHelper | [GameWindowHelper.md](GameWindowHelper.md) |
| DrawHelper | [DrawHelper.md](DrawHelper.md) |
| KeyParser | [KeyParser.md](KeyParser.md) |
| NativeInput | [NativeInput.md](NativeInput.md) |
