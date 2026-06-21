# Direct2DOverlayRenderer

## Purpose

Hosted Direct2D overlay window that renders the latest `OverlayFrame` on a dedicated STA UI thread. GPU-accelerated replacement for `WinFormsOverlayRenderer`.

## Key API

Implements `IOverlayRenderer` and `IHostedService`.

- `IsReady` — overlay HWND and render timer are running

Register via `AddDrawingDirect2D()` in `CS2Toolkit.Drawing.Direct2D`.

## Behavior

- Raw Win32 layered HWND (no WinForms dependency in this project) aligned to CS2 client bounds
- Disables click-through when `OverlayFrame.Interactive` is true (in-game menu open)
- 60 FPS present cap; skips frames when sequence unchanged
- WIC bitmap + Direct2D render target with GDI-compatible `UpdateLayeredWindow` alpha blit
- Never signals back to the game memory loop

## Dependencies

- `IOverlayFrameSource`
- `Direct2DOverlayHost` for rendering and blit
- `GameWindowHelper` for bounds
- Vortice.Direct2D1, DirectWrite, WIC

## Configuration

None (target FPS constant in implementation). Switch back to WinForms by calling `AddDrawingWinForms()` instead of `AddDrawingDirect2D()` in `Program.cs`.
