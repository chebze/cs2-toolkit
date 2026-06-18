# BombOverlay

## Purpose

Subscribes to `OnMemoryRead` and renders the current bomb state on the `bomb-status` overlay layer.

## Displayed status

Only rendered when `MemoryState.IsInMatch` is `true` and `MemoryState.Bomb.IsVisible` is `true`.

### Carried / Equipped / On ground

```
Bomb
  Carried
```

### Planting / Planted

```
Bomb
  Planting
  Site: A
```

```
Bomb
  Planted
  Site: B
  Time left: 32s
```

### Defusing

```
Bomb
  Defusing
  Time left: 18s
  Kit: yes
  Time to defuse: 5s
  Will succeed: yes
```

## Configuration

Controlled via `appsettings.json` → `Toolkit:Overlay:BombStatus`:

| Setting | Default | Description |
|---------|---------|-------------|
| `X` | 16 | Left position |
| `Y` | 220 | Top position (below teammate stats) |
| `Color` | `#FFD166` | Text color (HTML hex) |
| `FontSize` | 14 | Font size in points |

## Layer

- Name: `bomb-status`
- Z-Index: `100`

## Lifecycle

Registered as both a singleton and `IHostedService`. Mirrors `TeammateOverlay` architecture.
