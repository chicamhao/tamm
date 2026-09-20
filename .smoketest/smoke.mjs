#!/usr/bin/env node
// WebGL build smoke test. Spawns its own HTTPS server + headless Chromium,
// loads the built player, and fails (exit 1) unless Unity boots cleanly with
// no failed HTTP responses and no console errors.
//
// Usage:
//   node smoke.mjs [--chrome <path>] [--url <url>] [--port <n>] [--timeout <sec>]
//     --chrome     explicit Chromium/Chrome binary (else $CHROME_BIN, then auto-detect)
//     --url        page to test (default https://localhost:8443/)
//     --port       HTTPS port for the embedded server (default 8443)
//     --timeout    max seconds to wait for Unity boot (default 90)
//     --json       print only the machine-readable result object (still applies exit code)
import { spawn } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const args = process.argv.slice(2);
const argVal = name => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : undefined; };
const CHROME_EXPLICIT = argVal('--chrome');
const PORT = Number(argVal('--port') ?? '8443');
const TARGET_URL = argVal('--url') ?? `https://localhost:${PORT}/`;
const BOOT_TIMEOUT_MS = Number(argVal('--timeout') ?? '90') * 1000;
const JSON_ONLY = args.includes('--json') || process.env.GITHUB_ACTIONS === 'true';
const CDP_HTTP = `http://127.0.0.1:${Number(argVal('--cdp') ?? '9223')}`;

const sleep = ms => new Promise(r => setTimeout(r, ms));
const log = JSON_ONLY ? () => {} : (...a) => console.log(...a);

// ---------- resolve a Chromium binary ----------
function detectChrome() {
  if (CHROME_EXPLICIT && fs.existsSync(CHROME_EXPLICIT)) return CHROME_EXPLICIT;
  if (process.env.CHROME_BIN && fs.existsSync(process.env.CHROME_BIN)) return process.env.CHROME_BIN;
  const cacheRoot = path.join(process.env.HOME ?? '', 'Library/Caches/ms-playwright');
  const cacheRootLinux = path.join(process.env.HOME ?? '', '.cache/ms-playwright');
  const globs = [
    // playwright chromium (full + headless shell), any revision, mac arm64/x64 + linux
    ...fs.existsSync(cacheRoot) ? fs.readdirSync(cacheRoot).map(r =>
      ['chrome-mac-arm64', 'chrome-mac-x64'].map(a =>
        path.join(cacheRoot, r, a, 'Google Chrome for Testing.app', 'Contents', 'MacOS', 'Google Chrome for Testing'))) : [],
    ...fs.existsSync(cacheRootLinux) ? fs.readdirSync(cacheRootLinux).map(r =>
      [path.join(cacheRootLinux, r, 'chrome-linux', 'chrome')]) : [],
    ...fs.existsSync(cacheRoot) ? fs.readdirSync(cacheRoot).filter(r => r.startsWith('chromium_headless_shell')).map(r =>
      ['chrome-headless-shell-mac-arm64', 'chrome-headless-shell-mac-x64'].map(a =>
        path.join(cacheRoot, r, a, 'chrome-headless-shell'))) : [],
    ...fs.existsSync(cacheRootLinux) ? fs.readdirSync(cacheRootLinux).filter(r => r.startsWith('chromium_headless_shell')).map(r =>
      [path.join(cacheRootLinux, r, 'chrome-headless-shell-linux', 'chrome-headless-shell')]) : [],
    // system installs
    '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
    '/Applications/Chromium.app/Contents/MacOS/Chromium',
    '/Applications/Brave Browser.app/Contents/MacOS/Brave Browser',
    '/usr/bin/google-chrome', '/usr/bin/chromium', '/usr/bin/chromium-browser',
  ];
  for (const g of globs) if (g && fs.existsSync(g)) return g;
  return null;
}
const CHROME = detectChrome();
if (!CHROME) { console.error('No Chromium/Chrome found. Pass --chrome <path> or set CHROME_BIN.'); process.exit(2); }
if (!JSON_ONLY) log(`Using browser: ${CHROME}`);

// ---------- CDP client ----------
function makeCdp(wsUrl) {
  const ws = new WebSocket(wsUrl);
  const pending = new Map();
  let nextId = 1;
  const events = [];
  const onEvent = new Map();
  ws.addEventListener('message', e => {
    const msg = JSON.parse(e.data);
    if (msg.id && pending.has(msg.id)) {
      const { resolve, reject } = pending.get(msg.id);
      pending.delete(msg.id);
      if (msg.error) reject(new Error(`${msg.error.code}: ${msg.error.message}`));
      else resolve(msg.result);
    } else if (msg.method) {
      events.push(msg);
      for (const h of onEvent.get(msg.method) ?? []) {
        try { h(msg.params); } catch (err) { console.error('HANDLER_ERR', msg.method, err); }
      }
    }
  });
  const ready = new Promise(res => ws.addEventListener('open', () => res()));
  const send = (method, params = {}) => {
    const id = nextId++;
    const p = new Promise((resolve, reject) => pending.set(id, { resolve, reject }));
    ws.send(JSON.stringify({ id, method, params }));
    return p;
  };
  return { ready, send, events,
    on(method, h) { const a = onEvent.get(method); if (a) a.push(h); else onEvent.set(method, [h]); } };
}

// ---------- run ----------
let cdp = null;
let chrome = null;
const server = spawn('node', [path.join(HERE, 'server.mjs'), String(PORT)], { stdio: ['ignore', 'pipe', 'pipe'] });
server.on('exit', (code, sig) => console.error(`[server] exited code=${code} sig=${sig}`));
fs.writeFileSync('/tmp/smoke-server-stderr.log', '');
server.stderr.on('data', d => fs.appendFileSync('/tmp/smoke-server-stderr.log', d));

async function waitForServer(t) {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
  const t0 = Date.now();
  while (Date.now() - t0 < t) {
    try { if ((await fetch(`https://localhost:${PORT}/index.html`)).ok) return; } catch {}
    await sleep(300);
  }
  throw new Error('HTTPS server did not come up');
}

const failures = [];
let profileDir;
const results = { responses: [], console: [], exceptions: [], boot: null, finalState: null, shots: [] };

try {
  await waitForServer(15000);
  log(`Serving https://localhost:${PORT}/ (server pid ${server.pid})`);

  profileDir = fs.mkdtempSync('/tmp/cdp-smoke-');
  chrome = spawn(CHROME, [
    '--headless=new', '--no-sandbox', '--ignore-certificate-errors',
    `--remote-debugging-port=${CDP_HTTP.split(':')[2]}`,
    '--remote-allow-origins=*',
    `--user-data-dir=${profileDir}`,
    '--window-size=1280,720', '--hide-scrollbars', 'about:blank',
  ], { stdio: ['ignore', 'ignore', 'ignore'] });

  // wait for CDP
  const t0 = Date.now();
  while (Date.now() - t0 < 20000) {
    try { if ((await fetch(`${CDP_HTTP}/json/version`)).ok) break; } catch {}
    await sleep(300);
  }
  try { await fetch(`${CDP_HTTP}/json/version`); } catch { throw new Error('CDP endpoint did not come up'); }

  const tabs = await (await fetch(`${CDP_HTTP}/json`)).json();
  const tab = tabs.find(t => t.url === 'about:blank') ?? tabs[0];
  cdp = makeCdp(tab.webSocketDebuggerUrl);
  await cdp.ready;

  await cdp.send('Console.enable'); await sleep(200);
  await cdp.send('Network.enable'); await sleep(200);

  cdp.on('Console.messageAdded', p => results.console.push({ t: Date.now(), ...p.message }));
  cdp.on('Runtime.exceptionThrown', p => results.exceptions.push(p));
  cdp.on('Network.responseReceived', p => {
    results.responses.push({ url: p.response?.url, status: p.response?.status, mime: p.response?.mimeType });
  });

  await cdp.send('Page.navigate', { url: TARGET_URL });

  const bootT0 = Date.now();
  let booted = false;
  let lastProgress = '';
  while (Date.now() - bootT0 < BOOT_TIMEOUT_MS) {
    const { result } = await cdp.send('Runtime.evaluate', {
      expression: `(() => {
        const bar = document.getElementById('unity-progress-bar-full');
        const load = document.getElementById('unity-loading-container');
        const warn = document.getElementById('unity-warning');
        const canvas = document.getElementById('unity-canvas');
        const btn = document.getElementById('unity-fullscreen-button');
        return JSON.stringify({
          ready: document.readyState,
          progress: bar ? bar.style.width : null,
          loadingHidden: load ? getComputedStyle(load).display === 'none' || load.hidden : (load === null),
          warning: warn ? warn.textContent.trim() : '',
          canvas: canvas ? canvas.width + 'x' + canvas.height : null,
          fullscreenBtn: btn ? getComputedStyle(btn).display : null,
          title: (document.getElementById('unity-build-title')||{}).textContent || ''
        });
      })()`,
      returnByValue: true,
    });
    const state = JSON.parse(result.value);
    lastProgress = state.progress ?? lastProgress;
    if ((state.canvas && state.loadingHidden) || state.progress === '100%') { booted = true; results.boot = state; break; }
    if (state.warning) { results.boot = state; break; } // warning panel shown => stop early
    await sleep(1200);
  }
  if (!booted && !results.boot) results.boot = { timedOut: true, lastProgress };

  // let it run, then screenshot twice
  await sleep(3000);
  const shotTimes = [1500, 6000].map(ms => Date.now() + ms);
  for (const when of shotTimes) {
    await sleep(Math.max(0, when - Date.now()));
    try {
      const { data } = await cdp.send('Page.captureScreenshot', { format: 'png' });
      const file = path.join(HERE, `shot-${Date.now()}.png`);
      fs.writeFileSync(file, Buffer.from(data, 'base64'));
      results.shots.push({ file, bytes: fs.statSync(file).size });
    } catch {}
  }

  const { result } = await cdp.send('Runtime.evaluate', {
    expression: `(() => {
      const canvas = document.getElementById('unity-canvas');
      const load = document.getElementById('unity-loading-container');
      const bar = document.getElementById('unity-progress-bar-full');
      const warn = document.getElementById('unity-warning');
      return JSON.stringify({
        loadingVisible: load ? getComputedStyle(load).display : 'absent',
        progress: bar ? bar.style.width : null,
        canvas: canvas ? { w: canvas.width, h: canvas.height } : null,
        warning: warn ? warn.textContent.trim() : '',
      });
    })()`, returnByValue: true,
  });
  results.finalState = JSON.parse(result.value);

  // ---------- verdict ----------
  if (!results.boot || results.boot.timedOut) failures.push('Unity did not boot within timeout');
  if (results.finalState?.warning) failures.push(`unity-warning shown: ${results.finalState.warning}`);
  if (results.responses.length === 0) failures.push('no HTTP responses observed — page did not load');
  for (const r of results.responses.filter(r => r.status >= 400)) failures.push(`HTTP ${r.status}: ${r.url}`);
  const errs = results.console.filter(c => c.level === 'error');
  for (const e of errs) failures.push(`console error: ${e.text}`);
  if (results.exceptions.length > 0) failures.push(`${results.exceptions.length} runtime exception(s) thrown`);

} catch (err) {
  failures.push(`${err}`);
} finally {
  try { chrome?.kill('SIGKILL'); } catch {}
  try { server?.kill('SIGKILL'); } catch {}
  await sleep(500);
  if (typeof profileDir !== 'undefined') {
    spawn('pkill', ['-9', '-f', profileDir], { stdio: 'ignore' });
    spawn('rm', ['-rf', profileDir], { stdio: 'ignore' });
  }
}

const summary = {
  boot: results.boot,
  finalState: results.finalState,
  pass: failures.length === 0,
  failures,
  responses: results.responses.filter(r => /\.(br|js|html|wasm|data)/.test(r.url ?? '')).slice(0, 15),
  responsesSeen: results.responses.length,
  consoleErrors: results.console.filter(c => c.level === 'error').map(c => c.text).slice(0, 10),
  consoleWarnings: results.console.filter(c => c.level === 'warning').map(c => c.text).slice(0, 8),
  consoleTotal: results.console.length,
  exceptions: results.exceptions.length,
  screenshots: results.shots,
};
if (JSON_ONLY) console.log(JSON.stringify(summary, null, 2));
else {
  console.log(`\n=== WebGL smoke test: ${summary.pass ? 'PASS' : 'FAIL'} ===`);
  console.log(`boot: ${JSON.stringify(summary.boot)}`);
  console.log(`responses: ${summary.responsesSeen} resources, ${summary.responses.filter(r => r.status >= 400).length} bad`);
  for (const r of summary.responses.filter(r => r.status >= 400)) console.log(`  NON-200 ${r.status} ${r.url}`);
  console.log(`console: ${summary.consoleTotal} messages, ${summary.consoleErrors.length} errors, ${summary.consoleWarnings.length} warnings`);
  for (const e of summary.consoleErrors) console.log(`  ERR ${e.slice(0, 160)}`);
  for (const w of summary.consoleWarnings) console.log(`  WRN ${w.slice(0, 160)}`);
  console.log(`exceptions: ${summary.exceptions}`);
  console.log(failures.length ? `failures:\n  - ${failures.join('\n  - ')}` : 'no failures');
}
process.exit(summary.pass ? 0 : 1);