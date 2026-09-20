# WebGL Build Smoke Test

Loads the built WebGL player in headless Chromium over HTTPS with
production-equivalent headers (`.br` → `Content-Encoding: br`, `.wasm` →
`application/wasm`, range requests) and verifies that Unity actually boots and
renders. Exits non-zero on any regression.

## Requirements

- `node` ≥ 22 (uses the built-in `WebSocket` client)
- `openssl` (generates a throwaway self-signed cert on first run)
- a Chromium-family browser — auto-detected in this order:
  1. `--chrome <path>` argument
  2. `CHROME_BIN` env var
  3. Playwright browser cache (`~/Library/Caches/ms-playwright` or
     `~/.cache/ms-playwright`), any revision
  4. system Chrome / Chromium / Brave

## Usage

```bash
# from the repo root — serves Builds/WebGL and runs the test
node .smoketest/smoke.mjs            # human output, exit 0 = pass
node .smoketest/smoke.mjs --json    # machine-readable report (CI)

# options
node .smoketest/smoke.mjs --chrome /path/to/chromium   # explicit browser
node .smoketest/smoke.mjs --url  https://host/path     # test a remote deploy
node .smoketest/smoke.mjs --port 9443                  # different port
node .smoketest/smoke.mjs --timeout 120                # boot timeout (s)
```

`smoke.mjs` starts its own HTTPS server (`server.mjs`) and kills it afterwards —
no manual setup needed.

## What it verifies

- every HTTP response the player requests is `< 400`
- Unity boots: the loading overlay is dismissed, the `<canvas>` is live, and no
  `#unity-warning` panel shows
- the Brotli handshake succeeded — a broken
  `Content-Encoding: br`/`application/wasm` setup makes the loader throw
  *"still brotli-compressed"* errors, which fail the test
- zero console `error`-level messages and zero runtime exceptions
  (shader-stripping `warning`s from URP post-processing are expected and do
  not fail the run)
- two screenshots are written to `shot-*.png` for manual inspection
  (`analyze_png.py` does dependency-free pixel analysis if you can't view them)

## Exit codes

| code | meaning |
|---|---|
| 0 | pass — booted, no bad responses, no console errors |
| 1 | fail — see `failures[]` in the JSON report |
| 2 | no usable Chromium binary found |

## CI

GitHub Actions: `.github/workflows/ci.yml` builds the WebGL player with the
Unity CLI (`unity build --profile Web -o Builds/WebGL`), uploads it as an
artifact, then runs:

```bash
npx -y playwright install chromium   # headless-capable Chromium
node .smoketest/smoke.mjs --json
```

The smoke job fails the pipeline on any regression. To run the smoke job by
itself (no Unity license configured yet), use **workflow_dispatch** and upload
a build artifact named `webgl-build` (e.g. `gh run upload` / the `build` job
does this automatically).