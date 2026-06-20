# GameWindowHelper

## Purpose

Locates the CS2 game window and returns its client-area bounds for overlay positioning.

## Methods

### `GetTargetBounds()`

Returns CS2 client bounds when the game window is found, otherwise falls back to the primary monitor bounds.

### `TryGetCs2WindowHandle(out nint handle)`

Finds CS2 by window title (`Counter-Strike 2`) or by enumerating visible windows owned by the `cs2` process.

## Usage

Called by `ScreenOverlayManager` / `OverlayForm` to align the overlay with the game window instead of only using the primary screen rectangle.
