# IToolkitModule

## Purpose

Optional hook for future plugin modules to participate in startup orchestration.

## Key API

| Member | Description |
|--------|-------------|
| `Name` | Module display name |
| `Order` | Relative initialization order (lower runs first) |
| `InitializeAsync(IRuntimeOrchestrator, CancellationToken)` | Module bootstrap |

## Behavior

- No default implementations in v2; reserved for Phase 10+ plugin support.

## Dependencies

- `IRuntimeOrchestrator`

## Configuration

None.
