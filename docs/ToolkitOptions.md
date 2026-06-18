# ToolkitOptions

## Purpose

Root configuration model bound from `appsettings.json` under the `Toolkit` section.

## Properties

| Property | Default | Description |
|----------|---------|-------------|
| `InjectKey` | `F9` | Key to press for injection/attach |
| `MenuToggleKey` | `Insert` | Key to toggle settings menu |
| `PanicKey` | `F10` | Key to instantly shut down the app |
| `MemoryReadIntervalMs` | `100` | Memory poll interval |
| `FileLogging` | — | File log settings (see below) |

### Nested: FileLoggingOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Write logs to file |
| `Directory` | `logs` | Log output directory |
| `FileNamePrefix` | `cs2-toolkit` | Log file name prefix |
| `LogStatChanges` | `true` | Log alive/dead changes |
| `LogPlayerDetailsOnRoundEvents` | `true` | Log all players on round start/end |
| `Offsets` | — | Offset download URLs |
| `Overlay` | — | Overlay layout and styling |

## Nested: OffsetOptions

| Property | Description |
|----------|-------------|
| `OffsetsUrl` | URL to cs2-dumper `offsets.json` |
| `ClientDllUrl` | URL to cs2-dumper `client_dll.json` |

## Nested: OverlayOptions

| Property | Type | Description |
|----------|------|-------------|
| `EnemyLastSeen` | `SkeletonOverlayOptions` | Last-known enemy skeleton overlay |
| `TeammateStats` | `TextPanelOptions` | Teammate stat panel |
| `BombStatus` | `TextPanelOptions` | Bomb carrier status panel |
| `Clairvoyance` | `TextPanelOptions` | Clairvoyance tips panel |
| `Menu` | `MenuPanelOptions` | Settings menu panel |
| `InjectionPrompt` | `TextPanelOptions` | Top-right injection text |

## Nested: TextPanelOptions

| Property | Description |
|----------|-------------|
| `X`, `Y` | Screen position |
| `Color` | HTML hex color string |
| `FontSize` | Font size in points |

## Nested: MenuPanelOptions

Extends positioning with `BackgroundColor` and `TextColor` for the menu panel.

## Registration

```csharp
services.Configure<ToolkitOptions>(configuration.GetSection(ToolkitOptions.SectionName));
```

Injected via `IOptions<ToolkitOptions>` or `IOptionsMonitor<ToolkitOptions>`.
