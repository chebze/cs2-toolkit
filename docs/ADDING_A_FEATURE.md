# Adding a new feature service

Guide for extending CS2 Toolkit v2 with a combat or overlay capability.

## Overview

A feature typically spans four areas:

| Layer | Responsibility |
|-------|----------------|
| **Game** | Read memory → add mapped fields to `GameSnapshot` |
| **Services** | `IFeatureService` logic + optional `IOverlayPresenter` |
| **Configuration** | Profile DTOs in `Configuration.Abstractions` |
| **Input** (optional) | Keybind action in `ToolkitKeybindActions` + `FeatureRegistry` handler |

Services must **not** reference `CS2Toolkit.Game` or `CS2Toolkit.Input` implementations.

## Checklist

### 1. Model the snapshot (Game)

Add mapped state to `GameSnapshot` in `CS2Toolkit.Models.Abstractions` if the feature needs new game data.

Create or extend a reader under `CS2Toolkit.Game/Memory/` and wire it in `GameSnapshotMapper` / `GameSnapshotFactory`.

Document: `docs/{ReaderName}.md`, update `docs/GameSnapshot.md`.

### 2. Configuration

Add profile options to `ProfileSettings` or layered weapon settings in `Configuration.Abstractions`.

Defaults are resolved through `IActiveConfiguration.ResolveWeapon()` and `ToolkitSettings.Profile`.

### 3. Feature service (Services)

```csharp
// Services/Features/MyFeatureService.cs
internal sealed class MyFeatureService : IFeatureService
{
    public FeatureId Id => FeatureIds.MyFeature;
    public bool IsEnabled => true; // set true if service must tick when off (cleanup/reset)

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id)) { /* reset */ return; }
        // use context.Snapshot, context.WeaponSettings, context.Input
    }
}
```

Combat features that hold state (triggerbot, RCS) use `IsEnabled => true` and check `IFeatureState` inside `OnSnapshot` so they can reset when toggled off.

Register in `CS2Toolkit.Services/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.TryAddEnumerable(ServiceDescriptor.Singleton<IFeatureService, MyFeatureService>());
```

Add `FeatureIds.MyFeature` in `Services.Abstractions`.

### 4. Overlay presenter (optional)

```csharp
internal sealed class MyOverlayPresenter : IOverlayPresenter
{
    public int ZIndex => 100;
    public IReadOnlyList<DrawCommand> Present(GameSnapshot snapshot, ToolkitSettings settings) { ... }
}
```

Register as `IOverlayPresenter` in `AddToolkitServices()`. `OverlayComposer` merges all presenters each tick.

### 5. Keybind toggle (optional)

1. Add action id to `ToolkitKeybindActions` and `GlobalKeybinds` default key
2. Register in `KeybindMatcher`
3. Handle in `FeatureRegistry.OnKeybindActivated`
4. Expose in config UI keybinds page

### 6. Runtime toggle state

`IFeatureState` / `FeatureRuntimeState` tracks enabled flags. `FeatureRegistry.Toggle` updates state; F11 save persists via `ProfileSettingsSaver`.

### 7. Documentation

Per workspace rules, add or update `docs/{ClassName}.md` for every new or changed public type and add a row to [docs/README.md](README.md).

### 8. Verify

```bash
dotnet build CS2Toolkit.slnx
bash scripts/dependency-guard.sh
```

Manual: fabricate or attach, toggle feature, confirm overlay and behavior.

## Example: existing features

| Feature | Service | Presenter | Game reader |
|---------|---------|-----------|-------------|
| RCS | `RcsFeatureService` | `RcsOverlayPresenter` | `RcsReader` |
| Triggerbot | `TriggerbotFeatureService` | `TriggerbotOverlayPresenter` | `TriggerbotReader` |
| Enemy ESP | `EnemyEspFeatureService` | `EnemyEspOverlayPresenter` | bones in entity reader |
| Sound ESP | `SoundEspFeatureService` | `SoundEspOverlayPresenter` | `SoundEventReader` |
| Bullet tracers | `BulletTracerFeatureService` | `BulletTracerOverlayPresenter` | `BulletTracerReader` |

## Pipeline timing

`FeatureCoordinator` runs after attach (`IRuntimeOrchestrator` gate):

1. `IFeatureService.OnSnapshot` for each feature (combat + input simulation)
2. `IOverlayComposer.Compose` merges presenter draw commands
3. `IOverlayFrameSink.Publish` — non-blocking, latest-wins

See [ADR 002](adr/002-non-blocking-render.md) and [ADR 003](adr/003-snapshot-model.md).
