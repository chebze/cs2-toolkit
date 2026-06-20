# ProcessMemory

## Purpose

External memory access for the CS2 process using Win32 `ReadProcessMemory`. No DLL injection.

## Features

- Attach/detach to process by name
- Resolve `client.dll` module base address
- Typed reads: `Read<T>`, `ReadPtr`, `ReadString`

## Attach flow

```
AttachToProcess("cs2")
  → OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION)
  → Find client.dll base via Process.Modules
```

Returns `false` if process or module is not found.

## API

| Method | Description |
|--------|-------------|
| `AttachToProcess(string)` | Opens handle and resolves client.dll |
| `Detach()` | Closes process handle |
| `Read<T>(nint)` | Reads unmanaged struct/value |
| `ReadPtr(nint)` | Reads pointer (`nint`) |
| `ReadString(nint, int)` | Reads null-terminated UTF-8 string |

## Properties

| Property | Description |
|----------|-------------|
| `IsAttached` | Handle and client base are valid |
| `ClientBase` | Base address of `client.dll` |

## Security note

Requires sufficient privileges to read the target process. Running as the same user is typically sufficient for CS2.

## Registration

Singleton in DI. Shared by `ToolkitRuntime` (attach) and `GameMemoryReader` / `EntityResolver` (reads).
