#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SERVICES_CSPROJ="$ROOT/src/CS2Toolkit.Services/CS2Toolkit.Services.csproj"
API_CSPROJ="$ROOT/src/CS2Toolkit.API/CS2Toolkit.API.csproj"

fail() {
  echo "dependency-guard: $1" >&2
  exit 1
}

assert_no_reference() {
  local csproj="$1"
  local forbidden="$2"
  local label="$3"

  if grep -q "ProjectReference Include=.*${forbidden}" "$csproj"; then
    fail "$label must not reference $forbidden"
  fi
}

assert_no_reference "$SERVICES_CSPROJ" "CS2Toolkit.Game.csproj" "CS2Toolkit.Services"
assert_no_reference "$SERVICES_CSPROJ" "CS2Toolkit.Input.csproj" "CS2Toolkit.Services"
assert_no_reference "$API_CSPROJ" "CS2Toolkit.Services.csproj" "CS2Toolkit.API"

echo "dependency-guard: OK"
