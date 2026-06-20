# RcsController

## Purpose

Snapshot-driven recoil compensation. Applies relative mouse movement to counter aim punch while the user holds left mouse.

## Key API

| Member | Description |
|--------|-------------|
| `Process(context)` | Evaluates punch delta and moves mouse via `IInputSimulator` |
| `Reset()` | Clears punch tracking and bullet compensation state |

## Behavior

- Requires attached snapshot, in-match state, left mouse held, shots fired > 1, not scoped, and valid aim punch
- Computes pitch/yaw delta from successive `RcsState.AimPunch` samples
- Converts angles to mouse pixels using sensitivity, pitch/yaw scale, and CS2 yaw factor (`0.022`)
- Randomly skips compensation on the first compensated bullet and on subsequent bullets per weapon-layer chances
- Resets when not firing, scoped, on first shot, or when detached

## Dependencies

- `FeatureContext`, `RcsState`, `IInputSimulator`, `VirtualKeys`

## Configuration

Weapon profile `Rcs` layer: `Sensitivity`, `PitchScale`, `YawScale`, `FirstBulletCompensateChance`, `SubsequentBulletSkipChance`.
