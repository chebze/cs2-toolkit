# ADR 002: Non-blocking overlay rendering

## Status

Accepted

## Context

In the legacy app, overlay presenters could queue GDI+ work synchronously on paths shared with memory reading and combat logic. A slow or blocked renderer risked delaying triggerbot, RCS, and memory polling.

## Decision

Separate **produce** (pipeline thread) from **present** (UI thread):

1. Services build immutable `OverlayFrame` instances containing `DrawCommand` lists only — no `System.Drawing` on the pipeline thread.
2. `IOverlayFrameSink.Publish(frame)` overwrites a single latest slot (lock-free `Interlocked.Exchange`). Never blocks waiting for the UI.
3. `WinFormsOverlayRenderer` on a dedicated UI thread reads `IOverlayFrameSource.TryGetLatest` and executes draw commands. Dropped/skipped frames are acceptable.
4. Per-tick order in `FeatureCoordinator`: run all `IFeatureService` ticks (including combat + `IInputSimulator`) **before** `IOverlayComposer.Compose`.
5. Presenter exceptions are caught per-layer; partial frames still publish; game loop continues.

## Consequences

**Positive**

- Renderer stalls cannot block memory reads or combat timing
- Overlay composition has a bounded budget with warning logs
- Drawing backend can be replaced (DirectX, WPF) without touching Services

**Negative**

- UI may lag one or more frames behind game state (acceptable for ESP)
- No back-pressure from renderer to pipeline (by design)

## Related

- [OverlayFrame.md](../OverlayFrame.md)
- [FeatureCoordinator.md](../FeatureCoordinator.md)
- [WinFormsOverlayRenderer.md](../WinFormsOverlayRenderer.md)
