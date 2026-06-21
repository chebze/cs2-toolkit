# WinFormsOverlayRenderer

## Purpose

Hosted WinForms overlay window that renders the latest `OverlayFrame` on a dedicated UI thread.

## Key API

Implements `IOverlayRenderer` and `IHostedService`.

- `IsReady` — overlay form and timer are running

## Behavior

- STA UI thread with transparent click-through layered window aligned to CS2 client bounds
- Disables click-through when `OverlayFrame.Interactive` is true (in-game menu open)
- Uncapped present rate (1 ms timer); skips frames when sequence unchanged
- Back-buffer render + `UpdateLayeredWindow` alpha blit
- Never signals back to the game memory loop

## Dependencies

- `IOverlayFrameSource`
- `GameWindowHelper` for bounds

## Configuration

None (present rate uncapped; only skips when `OverlayFrame.Sequence` is unchanged).
