# MapNameReader

## Purpose

Reads the current map name from `matchmaking.dll` for active map selection on the visibility checker.

## Key API

```csharp
string? ReadCurrentMap(ProcessMemory memory, GameOffsets offsets)
```

## Behavior

- Tries `matchmaking.dll` (`dwGameTypes` + map name `CUtlString`), then `client.dll` `dwGlobalVars` at `0x180` and `0x230`
- Normalizes via `MapVisibilityChecker.NormalizeMapName` (strips garbage bytes, extracts `de_*` / `cs_*` / `ar_*` token)
- Returns null when module or name unavailable

## Dependencies

Called each memory tick by [GameMemoryReader.md](GameMemoryReader.md).
