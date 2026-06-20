# NativeInput

## Purpose

Global input polling via Win32 APIs. Used by `ToolkitRuntime` to publish keyboard and mouse events without requiring overlay focus.

## Methods

### `IsKeyDown(Keys key) → bool`

Polls `GetAsyncKeyState` for the given virtual key. Returns `true` while the key is held.

### `GetCursorPosition() → (int X, int Y)`

Returns current screen cursor coordinates via `GetCursorPos`.

### `GetPressedMouseButtons() → MouseButtons`

Returns a flags enum of all currently pressed mouse buttons (left, right, middle, X1, X2).

## Polling model

`ToolkitRuntime` polls every **16ms** (~60 Hz):

- Keyboard: edge detection (up→down = KeyDown/KeyPress, down→up = KeyUp).
- Mouse: position changes fire `OnMouseMove`; new button presses fire `OnMousePress`.

## Why global polling

The overlay is click-through by default, so it does not receive WinForms key/mouse events. Global polling ensures input works regardless of which window has focus.
