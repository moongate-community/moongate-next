#!/usr/bin/env bash
set -euo pipefail

version="${1:?version is required}"
root="$(git rev-parse --show-toplevel)"
release_dir="$root/artifacts/release"
publish_dir="$root/artifacts/publish"

rm -rf "$release_dir" "$publish_dir"
mkdir -p "$release_dir" "$publish_dir"

dotnet publish "$root/src/Moongate.Server/Moongate.Server.csproj" \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained false \
  --output "$publish_dir/linux-x64"

dotnet publish "$root/src/Moongate.Server/Moongate.Server.csproj" \
  --configuration Release \
  --runtime win-x64 \
  --self-contained false \
  --output "$publish_dir/win-x64"

tar -czf "$release_dir/moongate-linux-x64-v${version}.tar.gz" -C "$publish_dir/linux-x64" .

(
  cd "$publish_dir/win-x64"
  zip -qr "$release_dir/moongate-win-x64-v${version}.zip" .
)

ls -lh "$release_dir"
