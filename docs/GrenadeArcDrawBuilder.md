# GrenadeArcDrawBuilder

## Purpose

Builds overlay draw commands for grenade trajectory arcs, bounce markers, and landing rings.

## Key API

| Member | Description |
|--------|-------------|
| `Build(grenade, options, landingRadius, projector, viewMatrix, width, height, zIndex)` | Returns arc/landing draw commands |

## Behavior

- Draws arc segments as lines with point markers along the path
- Draws filled circles at bounce points and landing point
- Draws ground ring polygon at landing using `LandingMarkerRadiusUnits`

## Dependencies

- `GrenadeState`, `GrenadeVisualOptions`, `IWorldProjector`

## Configuration

Profile `Visuals.Grenade` colors and line widths; host `Grenade.LandingMarkerRadiusUnits`.
