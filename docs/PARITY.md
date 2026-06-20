# Feature parity checklist (v2 vs `_old/`)

Manual validation checklist for cutover. Mark items when verified in-game or via the config UI.

| Feature | v2 location | Parity |
|---------|-------------|--------|
| Offset download | `RuntimeOrchestratorHostedService` + `OffsetDownloader` | |
| Map preload | `RuntimeOrchestratorHostedService` + `MapDataService` | |
| Overlay renderer | `WinFormsOverlayRenderer` | |
| Inject (F9) | `InjectKeybindOrchestrator` + `IGameAttachment` | |
| Game memory loop | `GameMemoryLoop` (gated on attach) | |
| Keybind dispatcher | `KeybindDispatcher` | |
| Feature coordinator | `FeatureCoordinator` (gated on attach) | |
| Config web UI | `ApiHostService` + `CS2Toolkit.Frontend` | |
| Profile CRUD | `JsonConfigurationStore` + REST API | |
| Radar SSE | `RadarState` + `/api/radar/stream` | |
| Enemy ESP | `EnemyEspFeatureService` | |
| Sound ESP | `SoundEspFeatureService` | |
| Triggerbot + autostop | `TriggerbotFeatureService` + `AutoStopper` | |
| RCS | `RcsFeatureService` | |
| Aim helper | `AimHelperFeatureService` | |
| Clairvoyance | `ClairvoyanceAdvisor` | |
| Menu overlay | `MenuOverlayPresenter` | |
| Status toasts | `StatusToastStore` | |
| Panic (F10) | `FeatureRegistry` → detach + shutdown | |
| Save settings (F11) | `ProfileSettingsSaver` → profile store | |
| Legacy `store.json` import | `LegacySettingsMigrator.TryMigrateLegacyStore` | |
| Legacy `appsettings.json` import | `LegacySettingsMigrator.MigrateFromLegacyAppSettings` | |
