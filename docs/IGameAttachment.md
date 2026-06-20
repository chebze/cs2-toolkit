# IGameAttachment

## Purpose

Contract for attaching to and detaching from the CS2 process.

## Key API

- `IsAttached`
- `TryAttach(processName = "cs2")`
- `Detach()`

## Behavior

Implemented by `GameAttachmentService` in `CS2Toolkit.Game`.

## Dependencies

None (abstractions only).

## Configuration

Inject/attach key is configured in profile keybinds; Runtime wires the key to `TryAttach`.
