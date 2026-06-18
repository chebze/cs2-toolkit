# OverlayLayer

## Purpose

A named drawing surface within `ScreenOverlayManager`. Holds a thread-safe queue of draw actions that execute once per render frame.

## Properties

| Property | Description |
|----------|-------------|
| `Name` | Unique layer identifier |
| `ZIndex` | Render order (lower = behind) |

## Methods

### `QueueDraw(Action<Graphics> drawAction)`

Sets the layer's draw content. The action renders **every frame** until replaced by another `QueueDraw` call or cleared with `ClearQueue`.

### `ClearQueue()`

Removes the layer's draw content so nothing is rendered.

### `Render(Graphics graphics)`

Invokes the current draw action if one is set.

## Usage pattern

```csharp
var layer = overlayManager.GetOrCreateLayer("my-layer", zIndex: 200);

layer.QueueDraw(g =>
{
    g.DrawString("Hello", font, brush, x, y);
});
```

Call `QueueDraw` again whenever the content changes. You do not need to re-queue every frame for static text.
