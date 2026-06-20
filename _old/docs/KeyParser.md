# KeyParser

## Purpose

Parses keyboard key names from configuration strings into `System.Windows.Forms.Keys` values.

## API

### `Parse(string keyName) → Keys`

- Uses `Enum.TryParse<Keys>` (case-insensitive).
- Returns `Keys.None` for empty or invalid names.

## Examples

| Input | Result |
|-------|--------|
| `"F9"` | `Keys.F9` |
| `"insert"` | `Keys.Insert` |
| `""` | `Keys.None` |

## Usage

- `ToolkitRuntime` — inject key from `Toolkit:InjectKey`
- `MenuOverlay` — menu toggle from `Toolkit:MenuToggleKey`

Invalid inject keys cause a fatal startup error in `ToolkitRuntime`.
