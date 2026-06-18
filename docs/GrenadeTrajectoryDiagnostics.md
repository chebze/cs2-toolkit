# GrenadeTrajectoryDiagnostics

## Purpose

Resolver result wrapper carrying snapshot plus human-readable status for logging.

## Properties

| Property | Description |
|----------|-------------|
| `Snapshot` | `GrenadeTrajectorySnapshot` |
| `Status` | Diagnostic string (e.g. `active:game-trail`, `inactive:no-grenade`) |

## Behavior

Logged by `GrenadeTrajectoryTracker` when `Overlay:GrenadeTrajectory:LogDiagnostics` is true, throttled by `LogDiagnosticsIntervalMs`.
