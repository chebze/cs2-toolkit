# Direct2DOverlayHost

## Purpose

Owns the Direct2D off-screen render target and layered-window blit for the overlay. Renders `DrawCommand` lists into a WIC bitmap via Direct2D, then presents through `UpdateLayeredWindow`.

## Key API

- `PresentFrame(nint hwnd, OverlayBounds bounds, OverlayFrame frame)` — render and blit when sequence changes
- `LastRenderedSequence` — last consumed frame sequence
- `Dispose()` — releases D2D/WIC resources and brush cache

## Behavior

- Creates or resizes a `IWICBitmap` (32bpp PBGRA) and `ID2D1RenderTarget` (GDI-compatible) when dimensions change
- Clears to transparent, executes `DrawCommandExecutor`, then uses `ID2D1GdiInteropRenderTarget` + `UpdateLayeredWindow`
- Invalidates cached solid brushes when the render target is recreated
- Skips work when `frame.Sequence` matches the last rendered sequence

## Dependencies

- `ID2D1Factory1`, `IWICImagingFactory`, `IDWriteFactory`
- `DrawCommandExecutor`, `Direct2DResourceCache`

## Configuration

None.
