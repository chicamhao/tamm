#!/usr/bin/env bash
# Export a WebGL build (CLI-driven; no editor clicks needed beyond the one-time Pages toggle).
# Usage: ./scripts/build-web.sh
# Output: Builds/WebGL/ (worlds, loader, wasm).
#
# Prereq: close the Unity editor first — batch mode exits immediately while it holds the project lock.
set -euo pipefail
cd "$(dirname "$0")/.."

# Build editor. Uses the committed BuildWebCLI editor script + the "Web" build profile.
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.7.0b1/Unity.app/Contents/MacOS/Unity}"

"$UNITY_EDITOR" \
  -batchmode -quit -projectPath . -executeMethod BuildWebCLI.Build \
  -logFile /tmp/unity_webgl.log

echo "WebGL build in Builds/WebGL/ — deploy with ./scripts/deploy-gh-pages.sh"

# Reminder: compression is brotli (webGLCompressionFormat: 0 in Assets/Settings/Build Profiles/Web.asset).