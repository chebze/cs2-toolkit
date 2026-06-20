# GameSnapshot

## Purpose

Immutable per-tick view of mapped CS2 game state. The central contract consumed by services, overlays, and API presenters.

## Key API

Record properties: attachment/match flags, `LocalPlayer`, `Players` (with optional `Bones` and spotted flags), `Round`, `Bomb`, `ViewMatrix`, sounds, grenades, clairvoyance tips, team alive counts.

`Detached` — static snapshot when not attached to the game process.

## Behavior

Produced exclusively by `CS2Toolkit.Game` mappers. `RecentSounds` contains enemy noise events detected on that tick via `SoundEventReader`. Services must not mutate or re-parse raw memory into this shape.

## Dependencies

Types from `CS2Toolkit.Models.Abstractions`: `Player`, `LocalPlayer`, `RoundState`, `BombState`, `ViewMatrix`, etc.

## Configuration

None.
