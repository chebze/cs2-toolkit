# CS2 Toolkit v2 — Roadmap

This document is the complete implementation checklist for rebuilding CS2 Toolkit from scratch. The legacy monolith lives in [`_old/`](_old/) for reference only and is excluded from the solution.

---

## Architecture goals

- **Abstractions-first**: every capability area has `{Library}.Abstractions` (contracts) and `{Library}` (default implementation). Consumers reference abstractions only; `CS2Toolkit.Runtime` wires implementations.
- **Mapped game state**: `CS2Toolkit.Game` reads memory and maps to `CS2Toolkit.Models`. Services never parse offsets or raw structs.
- **Input is separate**: `CS2Toolkit.Input` listens and simulates keyboard/mouse. Services decide *when*; Input performs *how*.
- **Rendering never blocks the pipeline**: the hot path (read → map → services → input) never waits on WinForms/GDI+. Overlays consume immutable `OverlayFrame` via a latest-wins mailbox; dropped frames are acceptable.
- **Unified configuration**: one persistence model and one runtime resolution path. No `appsettings.json` vs `store.json` split.
- **DRY features**: shared toggle/registry pattern instead of per-feature `*State` + `*ToggleService` + `*StatusOverlay` copies.

---

## Target solution layout

```
CS2Toolkit.slnx
src/
├── CS2Toolkit.Models.Abstractions/
├── CS2Toolkit.Models/
├── CS2Toolkit.Configuration.Abstractions/
├── CS2Toolkit.Configuration/
├── CS2Toolkit.Input.Abstractions/
├── CS2Toolkit.Input/
├── CS2Toolkit.Game.Abstractions/
├── CS2Toolkit.Game/
├── CS2Toolkit.Drawing.Abstractions/
├── CS2Toolkit.Drawing.WinForms/
├── CS2Toolkit.Services.Abstractions/
├── CS2Toolkit.Services/
├── CS2Toolkit.API.Abstractions/
├── CS2Toolkit.API/
├── CS2Toolkit.Runtime.Abstractions/
├── CS2Toolkit.Runtime/
└── CS2Toolkit.Frontend/          # React + shadcn/ui (no .Abstractions pair)
docs/                             # v2 class documentation
_old/                             # legacy reference (not in solution)
```

### Dependency rules

| Rule | Detail |
|------|--------|
| Services → Game/Input | **Forbidden** (abstractions only) |
| API → Services | **Forbidden** (use `Services.Abstractions`) |
| Abstractions → implementations | **Forbidden** |
| Runtime | **Only** project referencing all implementations |
| Pipeline → WinForms | **Forbidden** (no `Graphics`, `Control.Invoke`, `PresentFrame` on hot path) |

---

## Phase 0 — Archive and repository scaffold

- [x] Move entire legacy codebase to `_old/`
- [x] Add `_old/README.md` (reference only, not built)
- [x] Create empty `CS2Toolkit.slnx` excluding `_old/`
- [x] Update root `.gitignore` for v2 layout
- [x] Add root `README.md` with architecture overview and link to this roadmap
- [x] Add `Directory.Build.props` (TFM `net9.0-windows` where needed, nullable, analyzers)
- [x] Add `global.json` pinning .NET SDK if required by CI
- [x] Add dependency guard script or analyzer: fail build if `CS2Toolkit.Services` references `CS2Toolkit.Game` or `CS2Toolkit.Input`

---

## Phase 1 — Solution skeleton and abstractions stubs

Create all 16 .NET projects with correct `ProjectReference` edges. Each implementation project gets a `DependencyInjection/ServiceCollectionExtensions.cs` stub.

### 1.1 Create projects

- [x] `src/CS2Toolkit.Models.Abstractions`
- [x] `src/CS2Toolkit.Models`
- [x] `src/CS2Toolkit.Configuration.Abstractions`
- [x] `src/CS2Toolkit.Configuration`
- [x] `src/CS2Toolkit.Input.Abstractions`
- [x] `src/CS2Toolkit.Input`
- [x] `src/CS2Toolkit.Game.Abstractions`
- [x] `src/CS2Toolkit.Game`
- [x] `src/CS2Toolkit.Drawing.Abstractions`
- [x] `src/CS2Toolkit.Drawing.WinForms` (`UseWindowsForms`)
- [x] `src/CS2Toolkit.Services.Abstractions`
- [x] `src/CS2Toolkit.Services`
- [x] `src/CS2Toolkit.API.Abstractions`
- [x] `src/CS2Toolkit.API` (`FrameworkReference` ASP.NET Core)
- [x] `src/CS2Toolkit.Runtime.Abstractions`
- [x] `src/CS2Toolkit.Runtime` (`OutputType` WinExe)
- [x] `src/CS2Toolkit.Frontend` (Vite + React + TypeScript + shadcn/ui scaffold)

### 1.2 Wire solution

- [x] Add all projects to `CS2Toolkit.slnx`
- [x] Verify `_old/Cs2Toolkit.csproj` is **not** in solution
- [x] Solution builds with empty/stub types

### 1.3 Runtime boots

- [x] `CS2Toolkit.Runtime/Program.cs` — `Host.CreateDefaultBuilder`, call stub `Add*` extensions
- [x] Log "CS2 Toolkit v2 ready" on startup
- [x] Exit criteria: `dotnet build` succeeds; `dotnet run --project src/CS2Toolkit.Runtime` starts host (requires Windows Desktop runtime)

### 1.4 Documentation

- [x] Create `docs/README.md` index for v2
- [x] Document stub extension methods as classes are added

---

## Phase 2 — Models and Configuration

### 2.1 `CS2Toolkit.Models.Abstractions`

- [x] `IGameStateSource` — async stream or observable of snapshots
- [x] `IReadOnlyGameState` — latest snapshot accessor
- [x] `IGameLifecycle` — `CS2Toolkit.Game.Abstractions` (Phase 4)
- [x] Identifiers and enums: `PlayerId`, `WeaponId`, `Team`, `BoneId`, `WeaponType`, `WeaponCategory`, `SoundKind`, `BombStatus`, `EnemyEspMode`
- [x] `docs/` entries for public interfaces (key types; expand as needed)

### 2.2 `CS2Toolkit.Models`

- [x] `GameSnapshot` — in `Models.Abstractions` (shared contract for `IGameStateSource`)
- [x] `Player`, `LocalPlayer`, `Weapon`, `PlayerBones` — core records in `Models.Abstractions`
- [x] `RoundState`, `BombState`, `BombSitesInfo` — in `Models.Abstractions`
- [x] `Vector3`, `ViewMatrix` in `Models.Abstractions`; `ScreenPoint` in `Models`
- [x] `SoundEvent`, `GrenadeState`, `RadarSnapshot` — in `Models.Abstractions`
- [x] Feature-ready views: `EspTarget`, `AimTarget`, `TriggerbotEvaluation`
- [x] No offsets, Win32, or JSON attributes in model projects
- [x] `docs/` for key public types (expand incrementally)

### 2.3 `CS2Toolkit.Configuration.Abstractions`

- [x] Serializable DTOs: `ToolkitSettings`, `ConfigProfile`, `ProfileSettings`, layered weapon settings
- [x] `IConfigurationStore` — CRUD, import/export
- [x] `ISettingsResolver` — global → weapon type → weapon merge
- [x] `IActiveConfiguration` — current profile + resolved settings
- [x] `IConfigurationChangeNotifier` — change events
- [x] `IKeybindConfiguration` — hotkey definitions (storage, not listening)
- [x] `docs/` for interfaces and DTOs (key types)

### 2.4 `CS2Toolkit.Configuration`

- [x] `JsonConfigurationStore` → `data/configs/store.json`
- [x] `SettingsResolver` / weapon layering (port logic from `_old/Configuration/`)
- [x] `ActiveConfiguration` implementing `IActiveConfiguration`
- [x] `LegacySettingsMigrator` — one-time import from legacy `appsettings.json` format
- [x] `AddToolkitConfiguration()` DI extension
- [x] Host `appsettings.json` — host paths, offsets URLs, log level (no feature profile settings)
- [x] Exit criteria: load/save profiles; resolve layered weapon settings without game attached

---

## Phase 3 — Input

### 3.1 `CS2Toolkit.Input.Abstractions`

- [x] `IInputListener` — key down/up, mouse move/button
- [x] `IInputSimulator` — key press/release, mouse move, click
- [x] `IInputState` — modifier/key snapshot
- [x] `IKeybindMatcher` — match `KeybindDefinition` to events
- [x] `InputEvent`, `KeyCode`, `MouseButton`, `KeybindDefinition`
- [x] `docs/` entries

### 3.2 `CS2Toolkit.Input`

- [x] `Win32InputListener` (port from `_old/Services/ToolkitRuntime.cs` key loop + `_old/Events/`)
- [x] `Win32InputSimulator` (port from `_old/Utilities/NativeInput.cs`)
- [x] `KeybindDispatcher` — consolidated hotkey handling (replaces `*ToggleService` key logic)
- [x] `AddToolkitInput()` DI extension
- [x] Wire keybind definitions from `IActiveConfiguration`
- [x] Exit criteria: log hotkey press; simulate mouse move in isolation (no game)

---

## Phase 4 — Game pipeline

### 4.1 `CS2Toolkit.Game.Abstractions`

- [x] `IMapVisibility` — line-of-sight raycast (stub returns true until Phase 5)
- [x] `IMapCatalog` — current map name
- [x] `IOffsetProvider` — resolved offsets metadata (not raw layout)
- [x] `IGameAttachment` — process attach state
- [x] `IGameLifecycle` — offset/attach lifecycle
- [x] `docs/` entries

### 4.2 `CS2Toolkit.Game` — process and offsets

- [x] `ProcessMemory` — attach, module bases, read primitives (port `_old/Memory/ProcessMemory.cs`)
- [x] `OffsetDownloader` — remote fetch (port `_old/Offsets/`)
- [x] Internal `GameOffsets` — never public outside Game
- [x] `AddToolkitGame()` DI extension

### 4.3 `CS2Toolkit.Game` — readers and mappers

Split `_old/Memory/EntityResolver.cs` (~925 LOC) into focused components:

- [ ] `PlayerReader` — still inside `EntitySnapshotReader` (interim monolith)
- [ ] `BombReader` — still inside `EntitySnapshotReader`
- [ ] `RoundReader` — still inside `EntitySnapshotReader`
- [x] `ViewMatrixReader`
- [x] `MapNameReader`
- [x] `LocalPlayerReader` (weapon + pose; interim `WeaponReader`)
- [x] `SoundReader` — `SoundEventReader` maps pawn sound state → `SoundEvent` on snapshot
- [x] `GrenadeReader` — `GrenadeTrajectoryReader` maps resolver output → `GrenadeState`
- [x] `GameSnapshotMapper` — assembles `GameSnapshot`
- [x] `GameStatePublisher` implements `IGameStateSource`

### 4.4 `CS2Toolkit.Game` — loop

- [x] `GameMemoryLoop` as `IHostedService`
- [x] Configurable poll interval (`MemoryReadIntervalMs`, default 5)
- [x] `PeriodicTimer` poll loop (high-resolution `timeBeginPeriod` deferred)
- [x] Publish snapshot without synchronous multicast events (channel + latest slot)
- [x] Exit criteria: attach to CS2; log snapshot summary (player count, map, local weapon)
- [ ] Optional: diff logging vs `_old` `EntityResolver` output for regression
- [x] Inject keybind → `IGameAttachment.TryAttach()` via Runtime orchestration

---

## Phase 5 — Maps and visibility

- [x] Port `_old/Maps/` — `Cs2InstallLocator`, `MapPhysicsParser`, `MapDataService`
- [x] `MapRaycastIndex` / BVH (port from `_old/`)
- [x] `MapVisibilityService` implements `IMapVisibility`
- [x] Cache under `data/maps/`
- [x] Preload maps at startup (`MapPreloadHostedService` via `AddToolkitGame()`)
- [x] Exit criteria: LOS queries work on parsed map geometry (when meshes loaded)

---

## Phase 6 — Drawing (non-blocking)

### 6.1 `CS2Toolkit.Drawing.Abstractions`

- [x] `DrawCommand` hierarchy — line, rect, circle, text, polyline, image
- [x] `OverlayFrame` — immutable `Sequence`, `ProducedAt`, `IReadOnlyList<DrawCommand>`
- [x] `IOverlayFrameSink.Publish(OverlayFrame)` — overwrites previous; never blocks
- [x] `IOverlayFrameSource.TryGetLatest(out OverlayFrame)`
- [x] `IWorldProjector` — world → screen from `ViewMatrix`
- [x] `IOverlayRenderer` — `IHostedService`; UI thread only
- [x] `IOverlayViewport` — screen dimensions for composition
- [x] **No** `System.Drawing` in abstractions
- [x] `docs/` entries

### 6.2 `CS2Toolkit.Drawing.WinForms`

- [x] `LatestFrameOverlaySink` — lock-free single-slot (`Interlocked.Exchange`)
- [x] `WinFormsOverlayHost` — transparent topmost window (port `_old/Overlay/ScreenOverlayManager.cs`)
- [x] `WinFormsOverlayRenderer` — consumes `IOverlayFrameSource` only; executes `DrawCommand`s
- [x] Back-buffer + layered blit
- [x] `AddDrawingWinForms()` DI extension
- [x] Renderer drops/skips frames when behind; never signals back to pipeline

### 6.3 Pipeline integration

- [x] `IOverlayComposer` in Services merges layer commands by z-index into one `OverlayFrame`
- [x] `OverlayPipelineHostedService` publishes after reading latest `GameSnapshot`
- [x] Debug presenter: player boxes from `GameSnapshot` only (`DebugPlayerBoxPresenter`)
- [x] `Player.WorldPosition` added to snapshot for world-projected debug boxes
- [ ] Optional: automated stress test — game loop interval unchanged when renderer sleeps 100 ms

---

## Phase 7 — Services core

### 7.1 `CS2Toolkit.Services.Abstractions`

- [x] `IFeatureService` — `FeatureId`, `IsEnabled`, `OnSnapshot(GameSnapshot, ResolvedFeatureSettings)`
- [x] `IFeatureRegistry` — enable/disable, list features
- [x] `IOverlayComposer` — `Compose(...) → OverlayFrame`
- [x] `IOverlayPresenter` — per-feature `IReadOnlyList<DrawCommand>`
- [x] `IFeatureState` — runtime toggle state (ESP mode, TB auto-stop)
- [x] `FeatureContext` — snapshot + resolved settings bundle + `IInputSimulator`
- [ ] Optional per-feature ports: `ITriggerbotService`, `IRadarService`, etc.
- [x] `docs/` entries

### 7.2 `CS2Toolkit.Services` — infrastructure

- [x] `FeatureCoordinator` — polls `IReadOnlyGameState`, fans out to features, composes overlay
- [x] `FeatureRegistry` — wired to `KeybindDispatcher` from Input
- [x] `OverlayComposer` — merges presenter outputs with per-layer try/catch and 1 ms budget warning
- [x] Per-tick order: combat services → overlay composition → publish
- [x] Presenter exceptions caught; partial frame published; game loop unaffected
- [x] `AddToolkitServices()` DI extension
- [x] Dependency guard: no `CS2Toolkit.Game` or `CS2Toolkit.Input` implementation refs

### 7.3 Feature migration (ordered)

Each feature: logic in Services, mapping already in Game, config from `IActiveConfiguration`, draw via `IOverlayPresenter`, docs updated.

| # | Feature | Status |
|---|---------|--------|
| 7.3.1 | Keybind → feature registry | **Done** |
| 7.3.2 | Teammate overlay | **Done** |
| 7.3.3 | Bomb overlay | **Done** |
| 7.3.4 | Enemy ESP (last seen → full) | **Done** |
| 7.3.5 | Sound ESP | **Done** |
| 7.3.6 | Grenade arc | **Done** |
| 7.3.7 | Triggerbot + autostop | **Done** |
| 7.3.8 | RCS | **Done** |
| 7.3.9 | Aim helper | **Done** |
| 7.3.10 | Clairvoyance | **Done** |
| 7.3.11 | Radar state | **Done** |
| 7.3.12 | In-game menu overlay | Stub service only |
| 7.3.13 | Status toasts / system messages | Not started |

Per-feature checklist:

- [ ] No `CS2Toolkit.Game` or `CS2Toolkit.Input` implementation imports
- [ ] Works from fabricated `GameSnapshot` in isolation (manual or test)
- [ ] `docs/{ClassName}.md` written/updated
- [ ] `docs/README.md` index updated

---

## Phase 8 — API and Frontend

### 8.1 `CS2Toolkit.API.Abstractions`

- [ ] `IRadarStreamSource` / `IRadarSnapshotProvider`
- [ ] `IDashboardInfoProvider`
- [ ] API-specific request/response records if not reused from Configuration DTOs
- [ ] `docs/` entries

### 8.2 `CS2Toolkit.API`

Port endpoints from `_old/Web/ConfigWebHostService.cs`:

- [ ] `GET/POST/PUT/DELETE /api/configs*`
- [ ] `GET/PUT /api/keybinds`
- [ ] `GET /api/weapons`
- [ ] `GET /api/dashboard`
- [ ] `GET /api/radar/snapshot`
- [ ] `GET /api/radar/stream` (SSE)
- [ ] Static files from `wwwroot/`
- [ ] `AddToolkitApi()` — register endpoints only; no host start
- [ ] `docs/ConfigWebHostService.md` equivalent for v2 API host types

### 8.3 `CS2Toolkit.Runtime` — API host

- [ ] `ApiHostService` — starts Kestrel (`0.0.0.0:8080+`), uses API middleware
- [ ] Auto-open browser on start (optional, match `_old` behavior)

### 8.4 `CS2Toolkit.Frontend`

- [ ] Vite + React 19 + TypeScript + Tailwind + shadcn/ui
- [ ] MSBuild/npm build → copy `dist/` to `wwwroot/`
- [ ] Pages (port from `_old/ConfigUI/src/`):

| Route | Page |
|-------|------|
| `/` | Dashboard |
| `/configs` | Profile CRUD, import/export |
| `/triggerbot`, `/rcs`, `/aimhelper` | Layered weapon settings |
| `/esp`, `/visuals`, `/sound` | Visual tuning |
| `/keybinds` | Global hotkeys |
| `/radar` | Live minimap (SSE) |

- [ ] API client (`fetch` to same origin)
- [ ] Live config: `PUT` → `IConfigurationChangeNotifier` → services rebind
- [ ] Exit criteria: edit profile in browser; radar SSE works

---

## Phase 9 — Runtime orchestration and cutover

### 9.1 `CS2Toolkit.Runtime.Abstractions`

- [ ] `IStartupPhase` — ordered gates
- [ ] `IRuntimeOrchestrator`
- [ ] Optional `IToolkitModule` for future plugins
- [ ] `docs/` entries

### 9.2 `CS2Toolkit.Runtime` — startup sequence

Replace `_old/ToolkitRuntime` + `RuntimeGate`:

1. [ ] Download offsets (fatal on failure)
2. [ ] Parse/preload map collision meshes
3. [ ] Start overlay renderer (UI thread)
4. [ ] Signal overlay ready
5. [ ] Wait for CS2 + user attach (F9)
6. [ ] Start `GameMemoryLoop`
7. [ ] Start `KeybindDispatcher`
8. [ ] Start `FeatureCoordinator`
9. [ ] Start API host + open config UI

- [ ] `AddRuntimeOrchestration()` DI extension
- [ ] Panic shutdown (F10), save hotkey behavior defined (prefer profile store over legacy `appsettings.json`)

### 9.3 Migration and parity

- [ ] `LegacySettingsMigrator` imports `_old` `store.json` and `appsettings.json` formats
- [ ] Side-by-side parity checklist vs `_old` for each feature
- [ ] Remove reliance on `_old` for daily development

### 9.4 Validation

- [ ] Stress: renderer blocked 100 ms → game loop stays at target interval
- [ ] Stress: slow presenter → triggerbot/RCS still fire on time
- [ ] Dependency analyzer passes on CI
- [ ] Full in-game manual test pass

---

## Phase 10 — Documentation and polish

- [ ] Root `README.md` — build, run, architecture diagram
- [ ] `docs/README.md` — index of all v2 class docs
- [ ] Every new/changed C# class has `docs/{ClassName}.md` per workspace rules
- [ ] Architecture decision record (ADR) for: abstractions split, non-blocking render, snapshot model
- [ ] Contributor guide: how to add a new feature service

---

## DI registration pattern

Each implementation project exposes:

```csharp
public static IServiceCollection AddXxx(this IServiceCollection services);
```

`CS2Toolkit.Runtime/Program.cs` target shape:

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddToolkitModels()
        .AddConfiguration()
        .AddInput()
        .AddGame()
        .AddDrawingWinForms()
        .AddToolkitServices()
        .AddToolkitApi()
        .AddRuntimeOrchestration());
```

Only Runtime references implementation projects.

---

## Non-blocking render contract (mandatory)

| DO | DON'T |
|----|-------|
| Build immutable `OverlayFrame` on pipeline thread | Call `Graphics` / GDI+ on pipeline thread |
| `IOverlayFrameSink.Publish()` — O(1), overwrite latest | Unbounded queue of frames |
| Renderer reads `IOverlayFrameSource` only | Renderer reads `GameSnapshot` or trackers |
| Drop frames when behind | Block pipeline waiting for present |
| Run combat + `IInputSimulator` before compose | Compose overlays before triggerbot fires |
| Catch presenter errors; continue loop | Let presenter exception stop game loop |

---

## Swappability matrix (future implementations)

| Abstraction | Default | Alternatives |
|-------------|---------|--------------|
| `IInputListener` | Win32 hook/poll | Raw Input, window-scoped listener |
| `IInputSimulator` | Win32 SendInput | Driver-level injection |
| `IGameStateSource` | Live publisher | Replay/file provider for tests |
| `IMapVisibility` | BVH raycast | Simplified grid |
| `IOverlayRenderer` | WinForms | DirectX, WPF |
| `IConfigurationStore` | JSON file | SQLite, cloud sync |
| API host | In-process Kestrel | Out-of-process |

---

## Open decisions (confirm before or during Phase 1)

| # | Question | Recommendation |
|---|----------|----------------|
| 1 | Namespace: `CS2Toolkit` vs `Cs2Toolkit`? | `CS2Toolkit` (match project names) |
| 2 | DTOs in `.Abstractions` or implementation only? | DTOs in `.Abstractions` for Configuration and API |
| 3 | Input listener scope: global vs CS2-window only? | Global (match `_old` behavior) |
| 4 | Frontend: full shadcn redesign vs port `_old/ConfigUI`? | Port pages incrementally, restyle with shadcn |
| 5 | Dedicated `CS2Toolkit.Logging` project? | Fold into Runtime initially |
| 6 | Test projects in Phase 1? | Add after Phase 7 Services core |

---

## Legacy reference map

| `_old/` path | v2 destination |
|--------------|----------------|
| `Utilities/NativeInput.cs` | `CS2Toolkit.Input` |
| `Events/ToolkitEventBus` (keys) | `CS2Toolkit.Input` |
| `Events/ToolkitEventBus` (memory) | `IGameStateSource` |
| `Memory/ProcessMemory`, `EntityResolver` | `CS2Toolkit.Game` |
| `Memory/Triggerbot`, `AimHelper`, `RecoilCompensator` | `CS2Toolkit.Services` |
| `Maps/*` | `CS2Toolkit.Game` |
| `Configuration/*` | `Configuration.Abstractions` + `Configuration` |
| `Models/*` (domain) | `CS2Toolkit.Models` |
| `Models/GameOffsets` | `CS2Toolkit.Game` (internal) |
| `Overlay/*` | `Drawing.Abstractions` + `Drawing.WinForms` |
| `Web/*` | `API.Abstractions` + `API` + Runtime host |
| `ConfigUI/*` | `CS2Toolkit.Frontend` |
| `Program.cs`, `ToolkitRuntime` | `CS2Toolkit.Runtime` |
| `Services/*Overlay`, `*Toggle` | `CS2Toolkit.Services` + `Input` |

---

## Current status

| Item | Status |
|------|--------|
| Phase 0 — Archive | **Done** |
| Phase 1 — Skeleton | **Done** |
| Phase 2 — Models + Configuration | **Done** |
| Phase 3 — Input | **Done** |
| Phase 4 — Game pipeline | **Mostly done** (reader split deferred) |
| Phase 5 — Maps and visibility | **Done** |
| Phase 6 — Drawing (non-blocking) | **Done** (renderer stress test deferred) |
| Phase 7 — Services core | **Done** (feature migration 7.3.12+ pending) |
| Phases 8–10 | Not started |

Next step: **Phase 7.3.12** — In-game menu overlay.
