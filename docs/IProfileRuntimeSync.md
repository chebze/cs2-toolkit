# IProfileRuntimeSync

## Purpose

Process-wide lock for profile runtime transitions. Ensures `FeatureCoordinator` does not read `IActiveConfiguration` while an active profile switch or toggle apply is in progress.

## Key API

| Member | Description |
|--------|-------------|
| `Acquire()` | Returns an `IDisposable` lease; dispose to release the lock |

## Behavior

- Implemented by `ProfileRuntimeSync` (singleton).
- `ActiveProfileSwitcher` acquires the lock for profile id changes and API toggle apply.
- `FeatureCoordinator` holds the lock for the full feature tick and overlay compose path.

## Dependencies

None.

## Configuration

None.
