#!/usr/bin/env bash
set -euo pipefail

version="${1:?version is required}"
root="$(git rev-parse --show-toplevel)"
branch="${GITHUB_REF_NAME:-$(git branch --show-current)}"
image="tgiachi/moongate-next"

: "${DOCKERHUB_USERNAME:?DOCKERHUB_USERNAME is required}"
: "${DOCKERHUB_TOKEN:?DOCKERHUB_TOKEN is required}"

tags=(
  "--tag" "$image:$version"
)

if [[ "$branch" == "main" ]]; then
  tags+=("--tag" "$image:latest")
elif [[ "$branch" == "develop" ]]; then
  tags+=("--tag" "$image:develop")
fi

echo "$DOCKERHUB_TOKEN" | docker login --username "$DOCKERHUB_USERNAME" --password-stdin

docker buildx build \
  --file "$root/src/Moongate.Server/Dockerfile" \
  "${tags[@]}" \
  --push \
  "$root"
