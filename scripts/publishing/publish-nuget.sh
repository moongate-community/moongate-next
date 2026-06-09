#!/usr/bin/env bash
set -euo pipefail
shopt -s nullglob

root="$(git rev-parse --show-toplevel)"
packages=("$root"/artifacts/packages/*.nupkg)

: "${NUGET_API_KEY:?NUGET_API_KEY is required}"

if [[ "${#packages[@]}" -eq 0 ]]; then
  echo "No NuGet packages found under artifacts/packages."
  exit 0
fi

for package in "${packages[@]}"; do
  dotnet nuget push "$package" \
    --api-key "$NUGET_API_KEY" \
    --source "https://api.nuget.org/v3/index.json" \
    --skip-duplicate
done
