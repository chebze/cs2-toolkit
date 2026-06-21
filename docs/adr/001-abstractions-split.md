# ADR 001: Abstractions-first project split

## Status

Accepted

## Context

The legacy codebase was a single WinForms + services monolith (`_old/`). Dependencies were implicit: overlay code read game memory directly, configuration was split between `appsettings.json` and profile storage, and swapping implementations (input backend, config store, renderer) required wide refactors.

## Decision

Every capability area gets two projects:

- `{Library}.Abstractions` — contracts, DTOs, enums
- `{Library}` — default implementation and `AddXxx()` DI extension

`CS2Toolkit.Runtime` is the sole composition root referencing implementation projects. Consumers depend on abstractions only.

Enforced rules:

- `CS2Toolkit.Services` must not reference `CS2Toolkit.Game` or `CS2Toolkit.Input`
- `CS2Toolkit.API` must not reference `CS2Toolkit.Services` (implementation)
- Abstractions never reference implementations

`scripts/dependency-guard.sh` validates the Services and API edges locally.

## Consequences

**Positive**

- Clear boundaries for testing (fabricated `GameSnapshot`, stub input)
- Swappable implementations documented in ROADMAP swappability matrix
- Feature work stays in Services without offset/memory knowledge

**Negative**

- More projects and boilerplate (`AddToolkitXxx()` per library)
- DTOs live in `.Abstractions` assemblies (larger public surface)

## Related

- [ROADMAP.md](../../ROADMAP.md) — dependency rules
- [002-non-blocking-render.md](002-non-blocking-render.md)
