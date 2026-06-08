#!/bin/bash

set -euo pipefail

RELEASE="Debug"
IS_AOT="${IS_AOT:-false}"
ROOT_DIRECTORY=""
SERVER_ARGS=()

if [[ "$IS_AOT" == "true" ]]; then
  echo "AOT mode is not supported yet. Set IS_AOT=false."
  exit 1
fi

print_usage() {
  cat <<'EOF'
Usage: scripts/run_server.sh [options] [server args]

Options:
  -r, --root-directory <path>  Runtime root directory passed to Moongate.
  -h, --help                   Show this help.

Examples:
  scripts/run_server.sh --root-directory "$HOME/moongate"
  scripts/run_server.sh -r /tmp/moongate --debug
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
  -r | --root-directory)
    if [[ $# -lt 2 ]]; then
      echo "Missing value for $1" >&2
      exit 1
    fi

    ROOT_DIRECTORY="$2"
    shift 2
    ;;
  --root-directory=*)
    ROOT_DIRECTORY="${1#*=}"
    shift
    ;;
  -h | --help)
    print_usage
    exit 0
    ;;
  --)
    shift
    SERVER_ARGS+=("$@")
    break
    ;;
  *)
    SERVER_ARGS+=("$1")
    shift
    ;;
  esac
done

if [[ -n "$ROOT_DIRECTORY" ]]; then
  SERVER_ARGS=(--root-directory "$ROOT_DIRECTORY" "${SERVER_ARGS[@]}")
fi

# Detect OS and architecture
UNAME_OS="$(uname -s)"
UNAME_ARCH="$(uname -m)"

# Map architecture
case "$UNAME_ARCH" in
arm64 | aarch64) ARCH="arm64" ;;
x86_64) ARCH="x64" ;;
*)
  echo "Unsupported architecture: $UNAME_ARCH"
  exit 1
  ;;
esac

# Map operating system
case "$UNAME_OS" in
Darwin) RID="osx-$ARCH" ;;
Linux) RID="linux-$ARCH" ;;
MINGW* | MSYS* | CYGWIN*) RID="win-$ARCH" ;;
*)
  echo "Unsupported operating system: $UNAME_OS"
  exit 1
  ;;
esac

# Build and run
dotnet publish -r "$RID" -o dist -p:PublishAot=$IS_AOT -c "$RELEASE" src/Moongate.Server &&
  ./dist/Moongate.Server "${SERVER_ARGS[@]}" &&
  rm -rf dist
