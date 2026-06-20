# WinFormsOverlayRenderer

## Purpose

Hosted WinForms overlay window that renders the latest `OverlayFrame` on a dedicated UI thread.

## Key API

Implements `IOverlayRenderer` and `IHostedService`.

- `IsReady` — overlay form and timer are running

## Behavior

- STA UI thread with transparent click-through layered window aligned to CS2 client bounds
- 60 FPS present cap; skips frames when sequence unchanged
- Back-buffer render + `UpdateLayeredWindow` alpha blit
- Never signals back to the game memory loop

## Dependencies

- `IOverlayFrameSource`
- `GameWindowHelper` for bounds

## Configuration

None (target FPS constant in implementation).
