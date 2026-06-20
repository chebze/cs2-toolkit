# Legacy CS2 Toolkit (reference only)

This folder contains the original monolithic `Cs2Toolkit` application. It is **not built** as part of the v2 solution.

Use it as a behavioral reference when porting features to the new layered architecture under `src/`.

## Contents

- Single-project WinForms + Kestrel + React config UI
- External memory reading, overlays, combat assists, and web configuration
- Original documentation in `docs/`

## Building (optional, manual)

```bash
cd _old
dotnet build Cs2Toolkit.csproj
```

The v2 solution at the repository root does not include this project.
