# StartupPhase

## Purpose

Enumerates ordered startup phases for the CS2 Toolkit v2 host orchestrator.

## Values

| Phase | Description |
|-------|-------------|
| `Offsets` | Download and validate game offsets |
| `Maps` | Parse and cache map collision geometry |
| `Overlay` | Start the overlay renderer |
| `Attach` | Wait for CS2 process attach |
| `GameLoop` | Start the memory read loop |
| `Input` | Start keybind dispatcher |
| `Features` | Start feature coordinator |
| `Api` | Start configuration web host |

## Dependencies

Used by `IRuntimeOrchestrator` and gated hosted services across Game, Input, Services, and Runtime.

## Configuration

None.
