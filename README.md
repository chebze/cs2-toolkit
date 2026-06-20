# CS2 Toolkit

External CS2 toolkit rebuilt as a layered .NET solution. The original monolithic application is preserved in [`_old/`](_old/) for reference only.

## Status

v2 is under active development. See **[ROADMAP.md](ROADMAP.md)** for the full implementation checklist.

## Repository layout

```
CS2Toolkit.slnx          # v2 solution (_old/ excluded)
ROADMAP.md               # implementation plan
src/                     # v2 projects (to be created)
docs/                    # v2 class documentation
_old/                    # legacy reference codebase
```

## Building

```bash
dotnet build CS2Toolkit.slnx
bash scripts/dependency-guard.sh
dotnet run --project src/CS2Toolkit.Runtime/CS2Toolkit.Runtime.csproj
```

`CS2Toolkit.Runtime` targets `net9.0-windows` and requires the Windows Desktop runtime.

To build the legacy app for comparison:

```bash
cd _old
dotnet build
```
