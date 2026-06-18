# AppSettingsWriter

## Purpose

Writes the `Toolkit` section of `appsettings.json` atomically without disturbing other root keys.

## Key API

```csharp
void SaveToolkitSection(string filePath, ToolkitOptions toolkit)
```

## Behavior

1. Parses existing JSON or creates empty root object
2. Replaces `Toolkit` node with serialized options (indented)
3. Writes to `.tmp` file then moves with overwrite

## Dependencies

- `ToolkitOptions` serialization
- Called from [SettingsSaveService.md](SettingsSaveService.md)
