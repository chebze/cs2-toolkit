# CS2 Toolkit v2 — Documentation

Class-level documentation for the v2 codebase. Each public C# type should have a matching `docs/{ClassName}.md` file.

## Guides

| Doc | Description |
|-----|-------------|
| [ADDING_A_FEATURE.md](ADDING_A_FEATURE.md) | How to add a new feature service |
| [PARITY.md](PARITY.md) | Manual v2 vs `_old/` validation checklist |

## Architecture decision records

| ADR | Topic |
|-----|-------|
| [001-abstractions-split.md](adr/001-abstractions-split.md) | Abstractions-first project split |
| [002-non-blocking-render.md](adr/002-non-blocking-render.md) | Non-blocking overlay pipeline |
| [003-snapshot-model.md](adr/003-snapshot-model.md) | Mapped `GameSnapshot` model |

## Projects

| Project | Purpose |
|---------|---------|
| `CS2Toolkit.Models.Abstractions` | Domain contracts, enums, `GameSnapshot`, core records |
| `CS2Toolkit.Models` | Feature views (`EspTarget`, `AimTarget`) and parsers |
| `CS2Toolkit.Configuration.Abstractions` | Settings DTOs and store contracts |
| `CS2Toolkit.Configuration` | JSON persistence and profile resolution |
| `CS2Toolkit.Input.Abstractions` | Keyboard/mouse listen and simulate contracts |
| `CS2Toolkit.Input` | Win32 input implementation |
| `CS2Toolkit.Game.Abstractions` | Map visibility and lifecycle contracts |
| `CS2Toolkit.Game` | Memory reading and snapshot mapping |
| `CS2Toolkit.Drawing.Abstractions` | Overlay frame and draw commands |
| `CS2Toolkit.Drawing.WinForms` | WinForms/GDI+ overlay renderer (fallback) |
| `CS2Toolkit.Drawing.Direct2D` | Direct2D overlay renderer (default) |
| `CS2Toolkit.Services.Abstractions` | Feature service contracts |
| `CS2Toolkit.Services` | ESP, triggerbot, aim helper, etc. |
| `CS2Toolkit.API.Abstractions` | HTTP-facing ports |
| `CS2Toolkit.API` | REST endpoints and static file registration |
| `CS2Toolkit.Runtime.Abstractions` | Startup orchestration contracts |
| `CS2Toolkit.Runtime` | Composition root and host |
| `CS2Toolkit.Frontend` | React config UI (no class docs) |

## Dependency injection extensions

| Extension | Project |
|-----------|---------|
| `AddToolkitModels()` | `CS2Toolkit.Models` |
| `AddToolkitConfiguration()` | `CS2Toolkit.Configuration` |
| `AddToolkitInput()` | `CS2Toolkit.Input` |
| `AddToolkitGame()` | `CS2Toolkit.Game` |
| `AddDrawingWinForms()` | `CS2Toolkit.Drawing.WinForms` |
| `AddDrawingDirect2D()` | `CS2Toolkit.Drawing.Direct2D` |
| `AddToolkitServices()` | `CS2Toolkit.Services` |
| `AddToolkitApi()` | `CS2Toolkit.API` |
| `AddRuntimeOrchestration()` | `CS2Toolkit.Runtime` |

## Class docs by area

### Runtime orchestration

| Class | Doc |
|-------|-----|
| `StartupPhase` | [StartupPhase.md](StartupPhase.md) |
| `IStartupPhase` | [IStartupPhase.md](IStartupPhase.md) |
| `IRuntimeOrchestrator` | [IRuntimeOrchestrator.md](IRuntimeOrchestrator.md) |
| `IToolkitModule` | [IToolkitModule.md](IToolkitModule.md) |
| `RuntimeOrchestratorHostedService` | [RuntimeOrchestratorHostedService.md](RuntimeOrchestratorHostedService.md) |
| `ApiHostService` | [ApiHostService.md](ApiHostService.md) |

### Configuration

| Class | Doc |
|-------|-----|
| `JsonConfigurationStore` | [JsonConfigurationStore.md](JsonConfigurationStore.md) |
| `ActiveConfiguration` | [ActiveConfiguration.md](ActiveConfiguration.md) |
| `SettingsResolver` | [SettingsResolver.md](SettingsResolver.md) |
| `LegacySettingsMigrator` | [LegacySettingsMigrator.md](LegacySettingsMigrator.md) |
| `WeaponCatalog` | [WeaponCatalog.md](WeaponCatalog.md) |
| `ProfileSettingsSaver` | [ProfileSettingsSaver.md](ProfileSettingsSaver.md) |
| `ProfileSwitchHostedService` | [ProfileSwitchHostedService.md](ProfileSwitchHostedService.md) |

### Input

| Class | Doc |
|-------|-----|
| `Win32InputListener` | [Win32InputListener.md](Win32InputListener.md) |
| `Win32InputSimulator` | [Win32InputSimulator.md](Win32InputSimulator.md) |
| `KeybindDispatcher` | [KeybindDispatcher.md](KeybindDispatcher.md) |
| `IInputSimulator` | [IInputSimulator.md](IInputSimulator.md) |

### Game pipeline

| Class | Doc |
|-------|-----|
| `GameSnapshot` | [GameSnapshot.md](GameSnapshot.md) |
| `IGameStateSource` | [IGameStateSource.md](IGameStateSource.md) |
| `GameStatePublisher` | [GameStatePublisher.md](GameStatePublisher.md) |
| `GameMemoryLoop` | [GameMemoryLoop.md](GameMemoryLoop.md) |
| `GameAttachmentService` | [GameAttachmentService.md](GameAttachmentService.md) |
| `IGameAttachment` | [IGameAttachment.md](IGameAttachment.md) |
| `IGameLifecycle` | [IGameLifecycle.md](IGameLifecycle.md) |
| `IOffsetProvider` | [IOffsetProvider.md](IOffsetProvider.md) |
| `IMapCatalog` | [IMapCatalog.md](IMapCatalog.md) |
| `IMapVisibility` | [IMapVisibility.md](IMapVisibility.md) |
| `MapDataService` | [MapDataService.md](MapDataService.md) |
| `MapVisibilityService` | [MapVisibilityService.md](MapVisibilityService.md) |
| `MapPhysicsParser` | [MapPhysicsParser.md](MapPhysicsParser.md) |

### Drawing

| Class | Doc |
|-------|-----|
| `OverlayFrame` | [OverlayFrame.md](OverlayFrame.md) |
| `IOverlayFrameSink` | [IOverlayFrameSink.md](IOverlayFrameSink.md) |
| `WinFormsOverlayRenderer` | [WinFormsOverlayRenderer.md](WinFormsOverlayRenderer.md) |
| `Direct2DOverlayRenderer` | [Direct2DOverlayRenderer.md](Direct2DOverlayRenderer.md) |
| `Direct2DOverlayHost` | [Direct2DOverlayHost.md](Direct2DOverlayHost.md) |

### Services core

| Class | Doc |
|-------|-----|
| `IFeatureService` | [IFeatureService.md](IFeatureService.md) |
| `IFeatureState` | [IFeatureState.md](IFeatureState.md) |
| `FeatureRuntimeState` | [FeatureRuntimeState.md](FeatureRuntimeState.md) |
| `IFeatureRegistry` | [IFeatureRegistry.md](IFeatureRegistry.md) |
| `FeatureRegistry` | [FeatureRegistry.md](FeatureRegistry.md) |
| `FeatureContext` | [FeatureContext.md](FeatureContext.md) |
| `FeatureCoordinator` | [FeatureCoordinator.md](FeatureCoordinator.md) |
| `ActiveProfileSwitcher` | [ActiveProfileSwitcher.md](ActiveProfileSwitcher.md) |
| `IProfileRuntimeSync` | [IProfileRuntimeSync.md](IProfileRuntimeSync.md) |
| `ProfileRuntimeSync` | [ProfileRuntimeSync.md](ProfileRuntimeSync.md) |
| `OverlayComposer` | [OverlayComposer.md](OverlayComposer.md) |
| `DebugPlayerBoxPresenter` | [DebugPlayerBoxPresenter.md](DebugPlayerBoxPresenter.md) |

### Combat features

| Class | Doc |
|-------|-----|
| `TriggerbotReader` | [TriggerbotReader.md](TriggerbotReader.md) |
| `TriggerbotController` | [TriggerbotController.md](TriggerbotController.md) |
| `AutoStopper` | [AutoStopper.md](AutoStopper.md) |
| `TriggerbotOverlayPresenter` | [TriggerbotOverlayPresenter.md](TriggerbotOverlayPresenter.md) |
| `RcsReader` | [RcsReader.md](RcsReader.md) |
| `RcsController` | [RcsController.md](RcsController.md) |
| `RcsOverlayPresenter` | [RcsOverlayPresenter.md](RcsOverlayPresenter.md) |
| `AimHelperReader` | [AimHelperReader.md](AimHelperReader.md) |
| `AimHelperController` | [AimHelperController.md](AimHelperController.md) |
| `AimHelperOverlayPresenter` | [AimHelperOverlayPresenter.md](AimHelperOverlayPresenter.md) |

### Visual / ESP features

| Class | Doc |
|-------|-----|
| `EnemyEspTracker` | [EnemyEspTracker.md](EnemyEspTracker.md) |
| `EnemyEspOverlayPresenter` | [EnemyEspOverlayPresenter.md](EnemyEspOverlayPresenter.md) |
| `EnemyEspDrawBuilder` | [EnemyEspDrawBuilder.md](EnemyEspDrawBuilder.md) |
| `SoundEspWaveTracker` | [SoundEspWaveTracker.md](SoundEspWaveTracker.md) |
| `SoundEspOverlayPresenter` | [SoundEspOverlayPresenter.md](SoundEspOverlayPresenter.md) |
| `SoundEspDrawBuilder` | [SoundEspDrawBuilder.md](SoundEspDrawBuilder.md) |
| `SoundEventReader` | [SoundEventReader.md](SoundEventReader.md) |
| `BulletTracerReader` | [BulletTracerReader.md](BulletTracerReader.md) |
| `BulletTracerTracker` | [BulletTracerTracker.md](BulletTracerTracker.md) |
| `BulletTracerOverlayPresenter` | [BulletTracerOverlayPresenter.md](BulletTracerOverlayPresenter.md) |
| `BulletImpactEvent` | [BulletImpactEvent.md](BulletImpactEvent.md) |
| `GrenadeTrajectoryReader` | [GrenadeTrajectoryReader.md](GrenadeTrajectoryReader.md) |
| `GrenadeTrajectoryResolver` | [GrenadeTrajectoryResolver.md](GrenadeTrajectoryResolver.md) |
| `GrenadeTrajectorySimulator` | [GrenadeTrajectorySimulator.md](GrenadeTrajectorySimulator.md) |
| `GrenadeArcOverlayPresenter` | [GrenadeArcOverlayPresenter.md](GrenadeArcOverlayPresenter.md) |
| `GrenadeArcDrawBuilder` | [GrenadeArcDrawBuilder.md](GrenadeArcDrawBuilder.md) |
| `TeammateStatsOverlayPresenter` | [TeammateStatsOverlayPresenter.md](TeammateStatsOverlayPresenter.md) |
| `BombStatusOverlayPresenter` | [BombStatusOverlayPresenter.md](BombStatusOverlayPresenter.md) |
| `ClairvoyanceAdvisor` | [ClairvoyanceAdvisor.md](ClairvoyanceAdvisor.md) |
| `ClairvoyanceOverlayPresenter` | [ClairvoyanceOverlayPresenter.md](ClairvoyanceOverlayPresenter.md) |
| `MenuOverlayPresenter` | [MenuOverlayPresenter.md](MenuOverlayPresenter.md) |

### Radar and status

| Class | Doc |
|-------|-----|
| `RadarReader` | [RadarReader.md](RadarReader.md) |
| `RadarState` | [RadarState.md](RadarState.md) |
| `IRadarSnapshotProvider` | [IRadarSnapshotProvider.md](IRadarSnapshotProvider.md) |
| `IStatusToastPublisher` | [IStatusToastPublisher.md](IStatusToastPublisher.md) |
| `StatusToastStore` | [StatusToastStore.md](StatusToastStore.md) |
| `StatusToastOverlayPresenter` | [StatusToastOverlayPresenter.md](StatusToastOverlayPresenter.md) |
| `StatusToastOrchestrator` | [StatusToastOrchestrator.md](StatusToastOrchestrator.md) |

### API

| Class | Doc |
|-------|-----|
| `IDashboardInfoProvider` | [IDashboardInfoProvider.md](IDashboardInfoProvider.md) |
| `DashboardInfoProvider` | [DashboardInfoProvider.md](DashboardInfoProvider.md) |
| `IRadarStreamSource` | [IRadarStreamSource.md](IRadarStreamSource.md) |

### Models (selected)

| Class | Doc |
|-------|-----|
| `Player` | [Player.md](Player.md) |
| `PlayerBones` | [PlayerBones.md](PlayerBones.md) |
| `BoneReader` | [BoneReader.md](BoneReader.md) |

---

Legacy documentation for the monolith is in [`_old/docs/`](../_old/docs/).
