#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "dependency-guard"
bash "$ROOT/scripts/dependency-guard.sh"

echo "dotnet test (net9.0 libraries only)"
dotnet test "$ROOT/tests/CS2Toolkit.Models.Tests/CS2Toolkit.Models.Tests.csproj" --verbosity minimal
dotnet test "$ROOT/tests/CS2Toolkit.Configuration.Tests/CS2Toolkit.Configuration.Tests.csproj" --verbosity minimal
dotnet test "$ROOT/tests/CS2Toolkit.Services.Tests/CS2Toolkit.Services.Tests.csproj" --verbosity minimal
dotnet test "$ROOT/tests/CS2Toolkit.API.Tests/CS2Toolkit.API.Tests.csproj" --verbosity minimal

echo "tests: OK"
