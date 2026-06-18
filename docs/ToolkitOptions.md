# ToolkitOptions

## Purpose

Root configuration model bound from `appsettings.json` under the `Toolkit` section.

## Top-level properties

| Property | Default | Description |
|----------|---------|-------------|
| `InjectKey` | `F9` | Key to press for injection/attach |
| `MenuToggleKey` | `Insert` | Key to toggle settings menu |
| `PanicKey` | `F10` | Key to instantly shut down the app |
| `SaveSettingsKey` | `F11` | Key to write runtime state to `appsettings.json` |
| `MemoryReadIntervalMs` | `5` | Memory poll interval in milliseconds |
| `Offsets` | — | Offset download URLs |
| `Overlay` | — | Overlay layout and styling |
| `Rcs` | — | Recoil compensation (see [Rcs.md](Rcs.md)) |
| `Tb` | — | Triggerbot (see [Tb.md](Tb.md)) |
| `EnemyEsp` | — | Enemy skeleton ESP mode |
| `SoundEsp` | — | Sound ripple overlay toggle |
| `AimHelper` | — | Aim snap assist |
| `Clairvoyance` | — | Contextual tip thresholds |
| `EnemyNoise` | — | Sound wave appearance |
| `Maps` | — | Map mesh cache and discovery |
| `Grenade` | — | Grenade trajectory simulation |
| `FileLogging` | — | File log settings |

### Nested: EnemyEspOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `ToggleKey` | `F6` | Cycle ESP mode at runtime |
| `Mode` | `LastSeen` | `Disabled`, `LastSeen`, or `Full` |

### Nested: SoundEspOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `ToggleKey` | `F5` | Toggle sound ESP |
| `Enabled` | `true` | Initial enabled state |

### Nested: AimHelperOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `ToggleKey` | `F4` | Tap toggle; hold + arrows adjust FOV |
| `Enabled` | `false` | Initial enabled state |
| `ActivationKey` | `""` | Optional hold-to-activate key (empty = always on when enabled) |
| `PreferredBone` | `Head` | `Head`, `Neck`, or `Body` |
| `FovDegrees` | `3` | Angular target window |
| `MinFovDegrees` / `MaxFovDegrees` | `0.5` / `15` | FOV clamp range |
| `FovAdjustStepDegrees` | `0.25` | Arrow adjustment step |
| `FovAdjustRepeatIntervalMs` | `80` | Repeat rate while holding toggle + arrow |
| `FovCircleColor`, `FovCircleLineWidth`, `AssumedHorizontalFovDegrees` | — | Crosshair ring styling |

### Nested: MapOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `CacheDirectory` | `data/maps` | Parsed mesh cache relative to app base |
| `MapsDirectory` | `null` | Optional override for CS2 `maps` folder |

### Nested: GrenadeOptions

Physics and simulation tuning for grenade arc prediction. See [GrenadeTrajectorySimulator.md](GrenadeTrajectorySimulator.md).

### Nested: FileLoggingOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Write logs to file |
| `Directory` | `logs` | Log output directory |
| `FileNamePrefix` | `cs2-toolkit` | Log file name prefix |
| `LogStatChanges` | `true` | Log alive/dead changes |
| `LogPlayerDetailsOnRoundEvents` | `true` | Log all players on round start/end |

### Nested: OffsetOptions

| Property | Description |
|----------|-------------|
| `OffsetsUrl` | URL to cs2-dumper `offsets.json` |
| `ClientDllUrl` | URL to cs2-dumper `client_dll.json` |

### Nested: OverlayOptions

| Property | Type | Description |
|----------|------|-------------|
| `TargetFps` | `int` | Overlay refresh cap (`0` = uncapped) |
| `EnemyLastSeen` | `SkeletonOverlayOptions` | Enemy skeleton color/width |
| `TeammateStats` | `TextPanelOptions` | Teammate stat panel |
| `BombStatus` | `TextPanelOptions` | Bomb carrier status panel |
| `Clairvoyance` | `TextPanelOptions` | Clairvoyance tips panel |
| `Menu` | `MenuPanelOptions` | Settings menu panel |
| `InjectionPrompt` | `TextPanelOptions` | Top-right system messages |
| `RcsStatus`, `TbStatus`, `EspStatus`, `SoundEspStatus`, `AimHelperStatus` | status panels | Bottom-left feature labels |
| `GrenadeTrajectory` | `GrenadeOverlayOptions` | Arc and landing marker styling |

### Nested: TextPanelOptions

| Property | Description |
|----------|-------------|
| `Enabled` | Whether the panel draws |
| `X`, `Y` | Screen position |
| `Color` | HTML hex color string |
| `FontSize` | Font size in points |

### Nested: SkeletonOverlayOptions

| Property | Default | Description |
|----------|---------|-------------|
| `Color` | `#FF6B6B` | Skeleton line color |
| `LineWidth` | `1.5` | Skeleton line width in pixels |

## Registration

```csharp
services.Configure<ToolkitOptions>(configuration.GetSection(ToolkitOptions.SectionName));
```

Injected via `IOptions<ToolkitOptions>` or `IOptionsMonitor<ToolkitOptions>`.
