# ClairvoyanceOverlay

## Purpose

Subscribes to `OnMemoryRead` and renders contextual tips from `MemoryState.ClairvoyanceTips`.

## Display

Only rendered when `MemoryState.IsInMatch` is `true`.

```
Clairvoyance
  You should reload
  You should be sneaky
```

When no tips apply:

```
Clairvoyance
  No tips yet...
```

## Configuration

Controlled via `appsettings.json`:

- Panel: `Toolkit:Overlay:Clairvoyance` (`X`, `Y`, `Color`, `FontSize`)
- Logic thresholds: `Toolkit:Clairvoyance`

Default position is below the bomb overlay (`Y: 320`).

## Layer

- Name: `clairvoyance`
- Z-Index: `100`

## Lifecycle

Registered as both a singleton and `IHostedService`.
