# CS2 Toolkit v2 — Documentation

Class-level documentation for the v2 codebase. Each public C# type should have a matching `docs/{ClassName}.md` file.

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
| `CS2Toolkit.Drawing.WinForms` | WinForms overlay renderer |
| `CS2Toolkit.Services.Abstractions` | Feature service contracts |
| `CS2Toolkit.Services` | ESP, triggerbot, aim helper, etc. |
| `CS2Toolkit.API.Abstractions` | HTTP-facing ports |
| `CS2Toolkit.API` | REST endpoints and static file registration |
| `CS2Toolkit.Runtime.Abstractions` | Startup orchestration contracts |
| `CS2Toolkit.Runtime` | Composition root and host |

## Dependency injection extensions

| Extension | Project |
|-----------|---------|
| `AddToolkitModels()` | `CS2Toolkit.Models` |
| `AddToolkitConfiguration()` | `CS2Toolkit.Configuration` |
| `AddToolkitInput()` | `CS2Toolkit.Input` |
| `AddToolkitGame()` | `CS2Toolkit.Game` |
| `AddDrawingWinForms()` | `CS2Toolkit.Drawing.WinForms` |
| `AddToolkitServices()` | `CS2Toolkit.Services` |
| `AddToolkitApi()` | `CS2Toolkit.API` |
| `AddRuntimeOrchestration()` | `CS2Toolkit.Runtime` |

## Class docs

_Stub types from Phase 1 are listed below. Expand as implementation proceeds._

| Class | Doc |
|-------|-----|
| `StartupPhase` | [StartupPhase.md](StartupPhase.md) |
| `GameSnapshot` | [GameSnapshot.md](GameSnapshot.md) |
| `IGameStateSource` | [IGameStateSource.md](IGameStateSource.md) |
| `JsonConfigurationStore` | [JsonConfigurationStore.md](JsonConfigurationStore.md) |
| `ActiveConfiguration` | [ActiveConfiguration.md](ActiveConfiguration.md) |
| `SettingsResolver` | [SettingsResolver.md](SettingsResolver.md) |
| `Win32InputListener` | [Win32InputListener.md](Win32InputListener.md) |
| `Win32InputSimulator` | [Win32InputSimulator.md](Win32InputSimulator.md) |
| `KeybindDispatcher` | [KeybindDispatcher.md](KeybindDispatcher.md) |
| `IInputSimulator` | [IInputSimulator.md](IInputSimulator.md) |
| `GameStatePublisher` | [GameStatePublisher.md](GameStatePublisher.md) |
| `GameAttachmentService` | [GameAttachmentService.md](GameAttachmentService.md) |
| `GameMemoryLoop` | [GameMemoryLoop.md](GameMemoryLoop.md) |
| `IGameAttachment` | [IGameAttachment.md](IGameAttachment.md) |
| `IGameLifecycle` | [IGameLifecycle.md](IGameLifecycle.md) |
| `IOffsetProvider` | [IOffsetProvider.md](IOffsetProvider.md) |
| `IMapCatalog` | [IMapCatalog.md](IMapCatalog.md) |
| `IMapVisibility` | [IMapVisibility.md](IMapVisibility.md) |
| `MapDataService` | [MapDataService.md](MapDataService.md) |
| `MapVisibilityService` | [MapVisibilityService.md](MapVisibilityService.md) |
| `MapPhysicsParser` | [MapPhysicsParser.md](MapPhysicsParser.md) |
| `OverlayFrame` | [OverlayFrame.md](OverlayFrame.md) |
| `IOverlayFrameSink` | [IOverlayFrameSink.md](IOverlayFrameSink.md) |
| `WinFormsOverlayRenderer` | [WinFormsOverlayRenderer.md](WinFormsOverlayRenderer.md) |
| `OverlayComposer` | [OverlayComposer.md](OverlayComposer.md) |
| `DebugPlayerBoxPresenter` | [DebugPlayerBoxPresenter.md](DebugPlayerBoxPresenter.md) |
| `IFeatureService` | [IFeatureService.md](IFeatureService.md) |
| `IFeatureRegistry` | [IFeatureRegistry.md](IFeatureRegistry.md) |
| `FeatureContext` | [FeatureContext.md](FeatureContext.md) |
| `FeatureCoordinator` | [FeatureCoordinator.md](FeatureCoordinator.md) |
| `FeatureRegistry` | [FeatureRegistry.md](FeatureRegistry.md) |
| `TeammateStatsOverlayPresenter` | [TeammateStatsOverlayPresenter.md](TeammateStatsOverlayPresenter.md) |
| `BombStatusOverlayPresenter` | [BombStatusOverlayPresenter.md](BombStatusOverlayPresenter.md) |
| `EnemyEspOverlayPresenter` | [EnemyEspOverlayPresenter.md](EnemyEspOverlayPresenter.md) |
| `EnemyEspTracker` | [EnemyEspTracker.md](EnemyEspTracker.md) |
| `EnemyEspDrawBuilder` | [EnemyEspDrawBuilder.md](EnemyEspDrawBuilder.md) |
| `BoneReader` | [BoneReader.md](BoneReader.md) |
| `Player` | [Player.md](Player.md) |
| `PlayerBones` | [PlayerBones.md](PlayerBones.md) |
| `SoundEventReader` | [SoundEventReader.md](SoundEventReader.md) |
| `SoundEspOverlayPresenter` | [SoundEspOverlayPresenter.md](SoundEspOverlayPresenter.md) |
| `SoundEspWaveTracker` | [SoundEspWaveTracker.md](SoundEspWaveTracker.md) |
| `SoundEspDrawBuilder` | [SoundEspDrawBuilder.md](SoundEspDrawBuilder.md) |

Legacy documentation for the monolith is in [`_old/docs/`](../_old/docs/).
