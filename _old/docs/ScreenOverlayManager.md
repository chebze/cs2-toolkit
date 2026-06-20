# ScreenOverlayManager

## Purpose

Manages a full-screen, transparent, topmost, click-through overlay window. Provides named layers with z-index ordering for composited drawing.

## Features

- **Full-screen coverage** — spans the primary monitor bounds.
- **Transparent background** — magenta transparency key; only drawn content is visible.
- **Click-through** — `WS_EX_TRANSPARENT` by default; disabled when menu is open.
- **Game-window aligned** — sizes/positions to the CS2 client area when available.
- **Topmost refresh** — re-asserts `HWND_TOPMOST` on inject and every ~1s so fullscreen games do not cover the overlay.
- **Layer system** — any component can create a named layer with a z-index.
- **Render loop** — 1ms timer with optional FPS cap; frames are composited off-screen and presented via `UpdateLayeredWindow` (faster than `Invalidate` + transparency key).

## API

### `StartAsync(CancellationToken)`

Starts a dedicated STA UI thread running the overlay `Form`. Returns a task that completes when the window is created.

### `GetOrCreateLayer(string name, int zIndex)`

Returns an existing layer or creates a new one. Updates z-index if the layer already exists.

### `EnsureOnTop()`

Syncs overlay bounds to the CS2 window and forces the overlay above the game using `SetWindowPos(HWND_TOPMOST)`. Called after injection and periodically by the render timer.

### `SetInteractive(bool interactive)`

Toggles click-through. When `interactive` is `true`, the overlay receives mouse input (used by `MenuOverlay`).

### `Render(Graphics graphics)`

Called by the overlay form on paint. Renders all layers sorted by ascending z-index.

## Layer rendering model

Layers use a **persistent draw** model:

1. Subscriber calls `layer.QueueDraw(action)` to set or update content.
2. On every paint (~60 FPS), the current draw action executes.
3. Content stays visible until replaced or `ClearQueue()` is called.

Lower z-index layers render first; higher z-index layers render on top.

An **overlay FPS** counter (`NN FPS`) is drawn in the top-right corner after all layers, updated once per second from the paint loop.

Set `Toolkit:Overlay:TargetFps` to cap the overlay (`60` = minimum 60 FPS target). Default `0` runs uncapped as fast as the compositor allows.

## Built-in layers

| Layer | Owner | Z-Index |
|-------|-------|---------|
| `enemy-last-seen` | `EnemyOverlay` | 100 |
| `teammate-stats` | `TeammateOverlay` | 100 |
| `enemy-noise` | `EnemyNoiseOverlay` | 200 |
| `menu` | `MenuOverlay` | 500 |
| `system` | `ToolkitRuntime` | 1000 |

## Threading

All WinForms operations run on the overlay STA thread. `SetInteractive` marshals to the UI thread via `BeginInvoke`.
