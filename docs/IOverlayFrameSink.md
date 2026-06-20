# IOverlayFrameSink

## Purpose

Non-blocking mailbox for overlay frames from the hot pipeline to the UI renderer.

## Key API

- `void Publish(OverlayFrame frame)` — overwrites the previous frame; never blocks

## Behavior

Implemented by `LatestFrameOverlaySink` using `Interlocked.Exchange`.

## Dependencies

None (abstractions only).

## Configuration

None.
