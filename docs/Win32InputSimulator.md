# Win32InputSimulator

## Purpose

Simulates keyboard and mouse input via Win32 `SendInput`, and reads cursor/key state.

## Key API

Implements `IInputSimulator`.

## Behavior

Used by combat assist services (triggerbot, RCS, aim helper) through the abstraction — services never call Win32 directly.

## Dependencies

`Win32InputNative`

## Configuration

None.
