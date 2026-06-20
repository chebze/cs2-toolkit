# FileLoggerProvider

## Purpose

`ILoggerProvider` that forwards all `Microsoft.Extensions.Logging` output (Information level and above) to `FileLogWriter`.

## Behavior

- Categories use the standard .NET logger category name (e.g. `Cs2Toolkit.Services.ToolkitRuntime`)
- Same log file as `MatchLogger` structured events
- Only active when `FileLogging:Enabled` is `true`

## Registration

```csharp
services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
```

Registered alongside `FileLogWriter` in `Program.cs`.
