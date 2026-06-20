# IOffsetProvider

## Purpose

Loads and exposes metadata for remotely fetched CS2 memory offsets without leaking raw layout types.

## Key API

- `OffsetMetadata Metadata` — source label, retrieval time, validity flag
- `EnsureLoadedAsync` — downloads and parses offset JSON

## Behavior

Implemented by internal `OffsetProviderService`. Raw `GameOffsets` remain internal to `CS2Toolkit.Game`.

## Dependencies

- `OffsetDownloader` (internal)
- `ToolkitHostSettings.Offsets` URLs

## Configuration

```json
"Toolkit": {
  "Offsets": {
    "OffsetsUrl": "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.json",
    "ClientDllUrl": "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.json"
  }
}
```
