# DrawHelper

## Purpose

Static utility methods for overlay text rendering.

## Methods

### `ParseColor(string hex, Color fallback)`

Parses an HTML hex color string (e.g. `#FF6B6B`). Returns `fallback` on parse failure.

### `DrawTextBlock(Graphics, int x, int y, IEnumerable<string> lines, Color color, int fontSize)`

Draws multiple lines of bold Segoe UI text stacked vertically.

### `DrawTextTopRight(Graphics, string text, Color color, int fontSize, int margin)`

Draws a single line anchored to the top-right of the visible clip bounds. Used for injection prompts.

## Usage

Called from overlay `QueueDraw` delegates in `ToolkitRuntime`, `EnemyOverlay`, `TeammateOverlay`, and `MenuOverlay`.
