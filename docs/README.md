# CS2 Toolkit v2 — Documentation

Class-level documentation for the v2 codebase. Each public C# type should have a matching `docs/{ClassName}.md` file.

## Projects

| Project | Purpose |
|---------|---------|
| `CS2Toolkit.Models.Abstractions` | Domain contracts and enums |
| `CS2Toolkit.Models` | Immutable domain records (`GameSnapshot`, etc.) |
| `CS2Toolkit.Configuration.Abstractions` | Settings DTOs and store contracts |
| `CS2Toolkit.Configuration` | JSON persistence and profile resolution |
| `CS2Toolkit.Input.Abstractions` | Keyboard/mouse listen and simulate contracts |
| `CS2Toolkit.Input` | Win32 input implementation |
| `CS2Toolkit.Game.Abstractions` | Map visibility and lifecycle contracts |
| `CS2Toolkit.Game` | Memory reading and snapshot mapping |
| `CS2Toolkit.Drawing.Abstractions` | Overlay frame and draw commands |
| `CS2Toolkit.Drawing.WinForms` | WinForms overlay renderer |
| `CS2Toolkit.Services.Abstractions` | Feature service contracts |
| `CS2Toolkit.Services` | ESP, triggerbot, aim helper, etc. |
| `CS2Toolkit.API.Abstractions` | HTTP-facing ports |
| `CS2Toolkit.API` | REST endpoints and static file registration |
| `CS2Toolkit.Runtime.Abstractions` | Startup orchestration contracts |
| `CS2Toolkit.Runtime` | Composition root and host |

## Dependency injection extensions

| Extension | Project |
|-----------|---------|
| `AddToolkitModels()` | `CS2Toolkit.Models` |
| `AddToolkitConfiguration()` | `CS2Toolkit.Configuration` |
| `AddToolkitInput()` | `CS2Toolkit.Input` |
| `AddToolkitGame()` | `CS2Toolkit.Game` |
| `AddDrawingWinForms()` | `CS2Toolkit.Drawing.WinForms` |
| `AddToolkitServices()` | `CS2Toolkit.Services` |
| `AddToolkitApi()` | `CS2Toolkit.API` |
| `AddRuntimeOrchestration()` | `CS2Toolkit.Runtime` |

## Class docs

_Stub types from Phase 1 are listed below. Expand as implementation proceeds._

| Class | Doc |
|-------|-----|
| `StartupPhase` | [StartupPhase.md](StartupPhase.md) |

Legacy documentation for the monolith is in [`_old/docs/`](../_old/docs/).
