#!/usr/bin/env bash
set -euo pipefail

version="${1:?version is required}"
root="$(git rev-parse --show-toplevel)"
packages_dir="$root/artifacts/packages"

rm -rf "$packages_dir"
mkdir -p "$packages_dir"

dotnet pack "$root/Moongate.slnx" \
  --configuration Release \
  --no-restore \
  --output "$packages_dir" \
  /p:PackageVersion="$version" \
  /p:Version="$version" \
  /p:InformationalVersion="$version"

find "$packages_dir" -maxdepth 1 -type f \( -name "*.nupkg" -o -name "*.snupkg" \) -print | sort
