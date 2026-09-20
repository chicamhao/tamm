// Minimal HTTPS server for Builds/WebGL with production-equivalent headers.
// .br  -> Content-Encoding: br
// .wasm-> application/wasm
// Supports Range requests (needed for ranged fetches).
import https from 'node:https';
import fs from 'node:fs';
import { spawn } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../Builds/WebGL');
const PORT = Number.isInteger(Number(process.argv[1])) ? Number(process.argv[1]) : 8443;

const types = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.png': 'image/png',
  '.ico': 'image/x-icon',
  '.wasm': 'application/wasm',
};
const contentEncoding = { '.br': 'br', '.gz': 'gzip' };

const HERE = path.dirname(fileURLToPath(import.meta.url));
const keyPath = path.join(HERE, 'key.pem');
const certPath = path.join(HERE, 'cert.pem');
async function ensureCert() {
  const ok = () => fs.existsSync(keyPath) && fs.existsSync(certPath) &&
    fs.statSync(keyPath).size > 0 && fs.statSync(certPath).size > 0;
  if (ok()) return;
  const cp = spawn('openssl', ['req', '-x509', '-newkey', 'rsa:2048', '-keyout', keyPath, '-out', certPath,
    '-days', '2', '-nodes', '-subj', '/CN=localhost', '-addext', 'subjectAltName=DNS:localhost,IP:127.0.0.1'],
    { stdio: 'ignore' });
  const st = await cp.status;
  if (st !== 0 || !ok()) throw new Error(`openssl cert generation failed (exit ${st})`);
}
await ensureCert();
const server = https.createServer({
  key: fs.readFileSync(keyPath),
  cert: fs.readFileSync(certPath),
}, (req, res) => {
  let urlPath = decodeURIComponent(req.url.split('?')[0]);
  if (urlPath === '/') urlPath = '/index.html';
  const filePath = path.join(ROOT, path.normalize(urlPath));
  if (!filePath.startsWith(ROOT) || !fs.existsSync(filePath) || fs.statSync(filePath).isDirectory()) {
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('404');
    return;
  }
  const ext = path.extname(filePath);
  const baseExt = contentEncoding[ext] ? path.extname(filePath.slice(0, -ext.length)) : ext;
  const headers = {
    'Content-Type': types[baseExt] ?? types[ext] ?? 'application/octet-stream',
    'Access-Control-Allow-Origin': '*',
    'Cache-Control': 'no-store',
  };
  if (contentEncoding[ext]) headers['Content-Encoding'] = contentEncoding[ext];

  const size = fs.statSync(filePath).size;
  const range = req.headers['range'] ?? req.headers['Range'] ?? '';
  if (range) {
    const m = /bytes=(\d*)-(\d*)/.exec(range);
    if (m) {
      const start = m[1] ? Number(m[1]) : 0;
      const end = m[2] ? Number(m[2]) : size - 1;
      if (start <= end && end < size) {
        const buf = fs.readFileSync(filePath);
        const slice = buf.slice(start, end + 1);
        headers['Content-Range'] = `bytes ${start}-${end}/${size}`;
        headers['Accept-Ranges'] = 'bytes';
        headers['Content-Length'] = String(slice.length);
        res.writeHead(206, headers);
        res.end(slice);
        return;
      }
    }
  }
  headers['Accept-Ranges'] = 'bytes';
  headers['Content-Length'] = String(size);
  res.writeHead(200, headers);
  res.end(fs.readFileSync(filePath));
});

server.listen(PORT, () => console.log(`serving ${ROOT} on https://localhost:${PORT}`));