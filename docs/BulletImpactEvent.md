# BulletImpactEvent

## Purpose

Mapped record for a single detected gunshot tracer segment on a game tick.

## Key API

- `ShooterId` — player who fired
- `Kind` — `Local`, `Teammate`, or `Enemy`
- `Start` / `End` — world-space line endpoints
- `Timestamp` — detection time (UTC)

## Behavior

Produced by `BulletTracerReader` when `M_iShotsFired` increases for a pawn. Consumed by `BulletTracerTracker` on the services layer.

## Dependencies

`PlayerId`, `BulletTracerKind`, `Vector3` from `CS2Toolkit.Models.Abstractions`.

## Configuration

None.
