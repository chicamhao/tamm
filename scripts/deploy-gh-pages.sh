#!/usr/bin/env bash
# Publish Builds/WebGL to the gh-pages branch (served from the repo root).
# Usage: ./scripts/deploy-gh-pages.sh        (run ./scripts/build-web.sh first)
#
# One-time repo setup: Settings → Pages → "Deploy from a branch" → gh-pages / (root).
# Site: https://chicamhao.github.io/tamm/
# Browsers cache the build for 10 min (max-age=600) — hard-refresh or incognito after a deploy.
# First deploy takes ~1 min.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -d Builds/WebGL/Build ] || {
  echo "No build at Builds/WebGL/Build — run ./scripts/build-web.sh first" >&2
  exit 1
}

# 1. Decompress for GitHub Pages. Pages can't send Content-Encoding: br on static files, and
#    Unity's loader refuses brotli payloads it wasn't told to inflate — so strip the compression
#    before deploying (only needed when the build profile has compression on).
( cd Builds/WebGL/Build
  shopt -s nullglob
  for f in *.br; do brotli -d -f "$f" -o "${f%.br}"; rm "$f"; done )

( cd Builds/WebGL
  sed -i '' -e 's|"/WebGL.data.br"|"/WebGL.data"|' \
            -e 's|"/WebGL.framework.js.br"|"/WebGL.framework.js"|' \
            -e 's|"/WebGL.wasm.br"|"/WebGL.wasm"|' index.html )

# 2. gh-pages serves the build from the repo ROOT (Build/, TemplateData/, index.html —
#    not Builds/WebGL).
git checkout gh-pages
rm -rf Build TemplateData
cp -R Builds/WebGL/Build Build
cp -R Builds/WebGL/TemplateData TemplateData
cp Builds/WebGL/index.html index.html
git add -f Build TemplateData index.html   # /Build is gitignored on main — force
git commit -m "deploy"
git push origin gh-pages
git checkout main

# 3. Verify (optional): headless-render the live URL — the title should read
#    "Unity Web Player | starter" and a <canvas> must exist with no page errors.