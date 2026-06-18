# WorldToScreenHelper

## Purpose

Projects 3D world coordinates to 2D screen space using the CS2 view-projection matrix.

## Key API

| Method | Description |
|--------|-------------|
| `TryProject(world, viewMatrix, w, h, out screen)` | Standard perspective projection |
| `TryProjectGroundRing(center, radius, viewMatrix, w, h, segments, out points)` | Horizontal ground circle for ripples/landing |

## Behavior

- Returns false for points behind the camera
- Used by skeleton ESP, grenade arc, sound ripples, and aim helper

## Dependencies

- `Models.Vector3` world positions
- 16-float view matrix from [ViewMatrixHolder.md](ViewMatrixHolder.md)
