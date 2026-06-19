# CS2 Toolkit — Architecture Overview

## Features

| Feature | Default key | Description |
|---------|-------------|-------------|
| Inject / attach | `F9` | Attach to `cs2.exe` after launch |
| Settings menu | `Insert` | Toggle read-only settings overlay |
| Panic shutdown | `F10` | Detach, close overlay, exit immediately |
| Save settings | `F11` | Write runtime toggles back to `appsettings.json` |
| Sound ESP | `F5` | Enemy footstep/reload/jump ripples + bomb waves |
| Enemy ESP | `F6` | Cycle skeleton overlay: off → last seen → full |
| Triggerbot | `F7` | Auto-fire with humanized timing (see [Tb.md](Tb.md)) |
| RCS | `F8` | Recoil compensation while spraying (see [Rcs.md](Rcs.md)) |
| Aim helper | `F4` | Snap aim to visible enemy bones in FOV |

Additional overlays (no toggle key): teammate stats, bomb carrier, clairvoyance tips, grenade trajectory arc.

## Event-loop driven design

```
Program
  └── IHost (Generic Host + DI)
        ├── ToolkitRuntime           → offsets, map parsing, injection, input loop
        ├── GameMemoryReader         → memory poll (default 5ms)
        ├── MatchLogger              → round/file diagnostics
        ├── EnemyOverlay             → enemy skeleton ESP
        ├── TeammateOverlay          → teammate stat panel
        ├── BombOverlay              → bomb carrier panel
        ├── ClairvoyanceOverlay      → contextual tips
        ├── EnemyNoiseOverlay        → sound/bomb ground ripples
        ├── GrenadeOverlay           → grenade arc + landing marker
        ├── MenuOverlay              → settings menu (Insert)
        ├── RcsOverlay / RcsToggleService
        ├── TbOverlay / TbToggleService
        ├── EnemyEspStatusOverlay / EnemyEspToggleService
        ├── SoundEspStatusOverlay / SoundEspToggleService
        ├── AimHelperOverlay / AimHelperToggleService
        ├── SettingsSaveService      → F11 persist runtime state
        ├── ConfigWebHostService     → Kestrel config UI + REST API (:8080+)
        ├── ConfigManager            → multi-profile store
        ├── LiveConfigApplier        → live in-game config updates
        └── ConfigProfileSwitchService → per-profile switch hotkeys
```

## Event bus

All cross-component communication flows through `ToolkitEventBus`:

```
ToolkitRuntime ──OnKey* / OnMouse*──► MenuOverlay, toggle services, SettingsSaveService
GameMemoryReader ──OnMemoryRead──► stat overlays, status overlays, MatchLogger
EnemySoundTracker ──OnEnemyNoise──► EnemyNoiseOverlay
ToolkitRuntime ──OnInjectionStatus──► (subscribers as needed)
```

## Startup order

1. Download CS2 offsets (fatal on failure)
2. Start overlay window
3. Parse map collision meshes (or load cache); signal `RuntimeGate` when done
4. Signal overlay ready
5. Injection flow (wait for CS2 + inject key)
6. `GameMemoryReader` starts (gated by `RuntimeGate`)
7. Overlays and memory features run on each poll / draw frame

## Running

```bash
dotnet run
```

1. Start CS2.
2. Launch the toolkit (map parsing may take a moment on first run).
3. The **configuration UI** opens automatically in your browser (default `http://localhost:8080`). Use the dashboard URLs to access it from your phone on the same network.
4. Press **F9** when prompted to attach.
5. Use feature hotkeys above; status labels appear bottom-left after attach.
6. Edit settings in the web UI — changes apply **live in-game** without restart.
7. Open **Radar** (`/radar`) for a live minimap shareable via LocalTunnel.
8. Switch profiles with per-profile hotkeys configured in the Profiles page.

### Web configuration UI

| Page | Description |
|------|-------------|
| Dashboard | Active profile name, LAN access URLs |
| Profiles | Create/delete/import/export configs, default profile, switch hotkeys |
| Triggerbot / RCS / Aim Helper | Global, weapon-type, and per-weapon layered settings |
| ESP | Skeleton mode, player name/health/bounding box |
| Visuals | Grenade arc/point/impact/landing colors |
| Sound ESP | Wave vs static box animation |
| Keybinds | Global hotkeys (shared across profiles) |

Config profiles are stored in `data/configs/store.json`. Legacy `appsettings.json` is migrated on first run.

Map raycast features (triggerbot pre-fire visibility, aim helper LOS, grenade simulation) require collision meshes. The toolkit auto-discovers CS2's `maps` folder or loads cached meshes from `data/maps`. Set `Toolkit:Maps:MapsDirectory` to override discovery.

## Project structure

```
Configuration/   ToolkitOptions, AppSettingsWriter
Events/          ToolkitEventBus, event args
Logging/         FileLogWriter, FileLoggerProvider
Maps/            Map parsing, raycast index, CS2 install discovery
Memory/          Process memory, entity resolution, feature logic
Models/          DTOs, enums, offsets
Offsets/         OffsetDownloader
Overlay/         ScreenOverlayManager, drawing helpers
Runtime/         RuntimeGate
Services/        Hosted services, overlays, runtime state
Utilities/       DrawHelper, KeyParser, NativeInput, projection
docs/            Per-class documentation
```

## Class documentation index

### Entry & runtime

| Class | Doc |
|-------|-----|
| Program | [Program.md](Program.md) |
| ToolkitRuntime | [ToolkitRuntime.md](ToolkitRuntime.md) |
| RuntimeGate | [RuntimeGate.md](RuntimeGate.md) |
| ToolkitEventBus | [ToolkitEventBus.md](ToolkitEventBus.md) |
| ToolkitEvents | [ToolkitEvents.md](ToolkitEvents.md) |

### Configuration & persistence

| Class | Doc |
|-------|-----|
| ToolkitOptions | [ToolkitOptions.md](ToolkitOptions.md) |
| ConfigManager | [ConfigManager.md](ConfigManager.md) |
| ConfigWebHostService | [ConfigWebHostService.md](ConfigWebHostService.md) |
| LocalTunnelClient | [LocalTunnelClient.md](LocalTunnelClient.md) |
| LocalTunnelHostedService | [LocalTunnelHostedService.md](LocalTunnelHostedService.md) |
| RadarTracker | [RadarTracker.md](RadarTracker.md) |
| RadarState | [RadarState.md](RadarState.md) |
| RuntimeConfigProvider | [RuntimeConfigProvider.md](RuntimeConfigProvider.md) |
| LiveConfigApplier | [LiveConfigApplier.md](LiveConfigApplier.md) |
| AppSettingsWriter | [AppSettingsWriter.md](AppSettingsWriter.md) |
| ToolkitOptionsCollector | [ToolkitOptionsCollector.md](ToolkitOptionsCollector.md) |
| SettingsSaveService | [SettingsSaveService.md](SettingsSaveService.md) |

### Memory & models

| Class | Doc |
|-------|-----|
| GameMemoryReader | [GameMemoryReader.md](GameMemoryReader.md) |
| ProcessMemory | [ProcessMemory.md](ProcessMemory.md) |
| EntityResolver | [EntityResolver.md](EntityResolver.md) |
| ViewMatrixHolder | [ViewMatrixHolder.md](ViewMatrixHolder.md) |
| MemoryState | [MemoryState.md](MemoryState.md) |
| PlayerInfo | [PlayerInfo.md](PlayerInfo.md) |
| GameOffsets | [GameOffsets.md](GameOffsets.md) |
| OffsetDownloader | [OffsetDownloader.md](OffsetDownloader.md) |

### Enemy ESP & sound

| Class | Doc |
|-------|-----|
| EnemyEspState | [EnemyEspState.md](EnemyEspState.md) |
| EnemyEspMode | [EnemyEspMode.md](EnemyEspMode.md) |
| EnemyEspToggleService | [EnemyEspToggleService.md](EnemyEspToggleService.md) |
| EnemyEspStatusOverlay | [EnemyEspStatusOverlay.md](EnemyEspStatusOverlay.md) |
| EnemyOverlay | [EnemyOverlay.md](EnemyOverlay.md) |
| EnemyLastSeenTracker | [EnemyLastSeenTracker.md](EnemyLastSeenTracker.md) |
| EnemySoundTracker | [EnemySoundTracker.md](EnemySoundTracker.md) |
| EnemyNoiseOverlay | [EnemyNoiseOverlay.md](EnemyNoiseOverlay.md) |
| SoundEspState | [SoundEspState.md](SoundEspState.md) |
| SoundEspToggleService | [SoundEspToggleService.md](SoundEspToggleService.md) |
| SoundEspStatusOverlay | [SoundEspStatusOverlay.md](SoundEspStatusOverlay.md) |

### Combat assists

| Class | Doc |
|-------|-----|
| RecoilCompensator | [RecoilCompensator.md](RecoilCompensator.md) |
| RcsState | [RcsState.md](RcsState.md) |
| RcsOverlay | [RcsOverlay.md](RcsOverlay.md) |
| RcsToggleService | [RcsToggleService.md](RcsToggleService.md) |
| RCS (feature guide) | [Rcs.md](Rcs.md) |
| Triggerbot | [Triggerbot.md](Triggerbot.md) |
| AutoStopper | [AutoStopper.md](AutoStopper.md) |
| TbState | [TbState.md](TbState.md) |
| TbOverlay | [TbOverlay.md](TbOverlay.md) |
| TbToggleService | [TbToggleService.md](TbToggleService.md) |
| TB (feature guide) | [Tb.md](Tb.md) |
| AimHelper | [AimHelper.md](AimHelper.md) |
| AimHelperState | [AimHelperState.md](AimHelperState.md) |
| AimHelperBone | [AimHelperBone.md](AimHelperBone.md) |
| AimHelperOverlay | [AimHelperOverlay.md](AimHelperOverlay.md) |
| AimHelperToggleService | [AimHelperToggleService.md](AimHelperToggleService.md) |

### Grenades & maps

| Class | Doc |
|-------|-----|
| GrenadeTrajectoryTracker | [GrenadeTrajectoryTracker.md](GrenadeTrajectoryTracker.md) |
| GrenadeTrajectoryResolver | [GrenadeTrajectoryResolver.md](GrenadeTrajectoryResolver.md) |
| GrenadeTrajectorySimulator | [GrenadeTrajectorySimulator.md](GrenadeTrajectorySimulator.md) |
| GrenadeTrajectory | [GrenadeTrajectory.md](GrenadeTrajectory.md) |
| GrenadeTrajectoryDiagnostics | [GrenadeTrajectoryDiagnostics.md](GrenadeTrajectoryDiagnostics.md) |
| GrenadeOverlay | [GrenadeOverlay.md](GrenadeOverlay.md) |
| GrenadeArcDrawer | [GrenadeArcDrawer.md](GrenadeArcDrawer.md) |
| MapDataService | [MapDataService.md](MapDataService.md) |
| MapPhysicsParser | [MapPhysicsParser.md](MapPhysicsParser.md) |
| MapRaycastIndex | [MapRaycastIndex.md](MapRaycastIndex.md) |
| MapVisibilityChecker | [MapVisibilityChecker.md](MapVisibilityChecker.md) |
| Cs2InstallLocator | [Cs2InstallLocator.md](Cs2InstallLocator.md) |
| MapNameReader | [MapNameReader.md](MapNameReader.md) |

### Stat overlays & advisors

| Class | Doc |
|-------|-----|
| TeammateOverlay | [TeammateOverlay.md](TeammateOverlay.md) |
| BombOverlay | [BombOverlay.md](BombOverlay.md) |
| BombInfo | [BombInfo.md](BombInfo.md) |
| BombStatus | [BombStatus.md](BombStatus.md) |
| ClairvoyanceAdvisor | [ClairvoyanceAdvisor.md](ClairvoyanceAdvisor.md) |
| ClairvoyanceOverlay | [ClairvoyanceOverlay.md](ClairvoyanceOverlay.md) |
| MenuOverlay | [MenuOverlay.md](MenuOverlay.md) |
| RoundInfo | [RoundInfo.md](RoundInfo.md) |

### Overlay infrastructure

| Class | Doc |
|-------|-----|
| ScreenOverlayManager | [ScreenOverlayManager.md](ScreenOverlayManager.md) |
| OverlayLayer | [OverlayLayer.md](OverlayLayer.md) |
| SkeletonDrawer | [SkeletonDrawer.md](SkeletonDrawer.md) |
| GroundWaveDrawer | [GroundWaveDrawer.md](GroundWaveDrawer.md) |
| DrawHelper | [DrawHelper.md](DrawHelper.md) |
| WorldToScreenHelper | [WorldToScreenHelper.md](WorldToScreenHelper.md) |
| GameWindowHelper | [GameWindowHelper.md](GameWindowHelper.md) |

### Logging & utilities

| Class | Doc |
|-------|-----|
| MatchLogger | [MatchLogger.md](MatchLogger.md) |
| FileLogWriter | [FileLogWriter.md](FileLogWriter.md) |
| FileLoggerProvider | [FileLoggerProvider.md](FileLoggerProvider.md) |
| KeyParser | [KeyParser.md](KeyParser.md) |
| NativeInput | [NativeInput.md](NativeInput.md) |
| BoneHelper | [BoneHelper.md](BoneHelper.md) |
| BombSiteHelper | [BombSiteHelper.md](BombSiteHelper.md) |
