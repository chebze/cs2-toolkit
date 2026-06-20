# IInputSimulator

## Purpose

Abstraction for reading and simulating keyboard/mouse input.

## Key API

- `IsKeyDown`, `GetCursorPosition`, `GetPressedMouseButtons`
- `MoveMouseRelative`, `SetLeftButton`, `SetKeyState`

## Behavior

Implemented by `Win32InputSimulator` in `CS2Toolkit.Input`.

## Dependencies

Consumed by `CS2Toolkit.Services` (Phase 7+) via abstraction only.

## Configuration

None.
