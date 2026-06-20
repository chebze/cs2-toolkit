# MenuOverlay

## Purpose

Interactive settings menu rendered on the `menu` overlay layer. Toggled by a configurable hotkey and uses keyboard/mouse events from `ToolkitEventBus`.

## Toggle

- Default key: `Insert` (`Toolkit:MenuToggleKey`)
- Pressing the toggle key shows/hides the menu.
- When visible, `ScreenOverlayManager.SetInteractive(true)` disables click-through so the overlay can receive input.

## Displayed settings

The menu shows current configuration values (read-only display):

- Inject key
- Menu toggle key
- Panic key
- Memory read interval
- Enemy stats position, color, font size
- Teammate stats position, color, font size

## Event subscriptions

| Event | Behavior |
|-------|----------|
| `OnKeyPress` | Toggle menu visibility on menu key |
| `OnMouseMove` | Re-queue menu draw while visible |

## Configuration

`appsettings.json` → `Toolkit:Overlay:Menu`:

| Setting | Default | Description |
|---------|---------|-------------|
| `X` | 16 | Menu panel left |
| `Y` | 16 | Menu panel top |
| `BackgroundColor` | `#CC1E1E2E` | Semi-transparent panel |
| `TextColor` | `#FFFFFFFF` | Label color |
| `FontSize` | 13 | Menu font size |

## Layer

- Name: `menu`
- Z-Index: `500` (above stat overlays, below system messages)

## Extensibility

Future work can add editable controls by handling `OnMousePress` for click targets and writing changes back to configuration.
