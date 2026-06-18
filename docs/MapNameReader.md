# MapNameReader

## Purpose

Reads the current map name from `matchmaking.dll` for active map selection on the visibility checker.

## Key API

```csharp
string? ReadCurrentMap(ProcessMemory memory, GameOffsets offsets)
```

## Behavior

- Reads `dwGameTypes` + `mapName` string from matchmaking module
- Normalizes via `MapVisibilityChecker.NormalizeMapName`
- Returns null when module or name unavailable

## Dependencies

Called each memory tick by [GameMemoryReader.md](GameMemoryReader.md).
