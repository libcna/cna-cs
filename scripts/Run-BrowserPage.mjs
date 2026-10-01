#!/usr/bin/env node
// Serves a published .NET browser bundle and runs it in headless Chromium (SwiftShader WebGL2),
// printing the page console, until a console line contains MARKER or the page throws. A screenshot
// is taken when a line contains SHOT_MARKER, or at the end.
// Usage: NODE_PATH=<global node_modules with playwright> node Run-BrowserPage.mjs <bundle-dir>
//        <screenshot.png> [MARKER] [SHOT_MARKER]
// Chromium only gets WebGL with DISPLAY and WAYLAND_DISPLAY unset.
// CNA_ACTIONS scripts input, separated by ';', each at a time in ms after the page loads:
//   move:X,Y@T   press@T   release@T   key:NAME@T   down:NAME@T   up:NAME@T   reload@T   shot:LABEL@T
// (press/release are the left mouse button)
// A shot writes <screenshot>-LABEL.png beside the main screenshot.
import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { extname, join } from 'node:path';
import { createRequire } from 'node:module';

const [root, shot, marker = '', shotMarker = ''] = process.argv.slice(2);
// Without a marker the page runs this long and passes unless it threw.
const seconds = Number(process.env.CNA_RUN_SECONDS ?? 8);
const MIME = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript',
               '.wasm': 'application/wasm', '.json': 'application/json', '.dll': 'application/octet-stream',
               '.dat': 'application/octet-stream', '.pdb': 'application/octet-stream', '.webcil': 'application/octet-stream',
               '.xnb': 'application/octet-stream' };

const server = createServer(async (request, response) => {
    const path = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    try {
        const file = join(root, path === '/' ? '/index.html' : path);
        const body = await readFile(file);
        response.writeHead(200, { 'Content-Type': MIME[extname(file)] ?? 'application/octet-stream',
                                  'Cache-Control': 'no-store' });
        response.end(body);
    } catch {
        response.writeHead(404);
        response.end();
    }
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const url = `http://127.0.0.1:${server.address().port}/`;

const require = createRequire(import.meta.url);
const { chromium } = require(join(process.env.NODE_PATH, 'playwright'));
const browser = await chromium.launch({ headless: true,
    args: ['--use-gl=swiftshader', '--enable-unsafe-swiftshader', '--no-sandbox'] });
const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
// The canvas, not the viewport: it is the game's back buffer, the same size a desktop capture has.
const capture = path => page.locator('#canvas').screenshot({ path });
let shotTaken = false;
let resolveDone;
const done = new Promise(resolve => { resolveDone = resolve; });
page.on('console', async message => {
    const text = message.text();
    console.log(`[console.${message.type()}] ${text}`);
    if (!shotTaken && shotMarker && text.includes(shotMarker)) {
        shotTaken = true;
        await capture(shot);
    }
    if (marker && text.includes(marker)) resolveDone('done');
});
page.on('pageerror', error => { console.log(`[pageerror] ${error.message}`); resolveDone('error'); });

let status = 0;
try {
    await page.goto(url);
    const actions = (process.env.CNA_ACTIONS ?? '').split(';').map(a => a.trim()).filter(Boolean);
    const started = Date.now();
    const runActions = (async () => {
        for (const action of actions) {
            const [what, at] = action.split('@');
            const wait = Number(at ?? 0) - (Date.now() - started);
            if (wait > 0) await new Promise(resolve => setTimeout(resolve, wait));
            const [verb, argument = ''] = what.split(/:(.*)/s);
            console.log(`[action] ${action}`);
            if (verb === 'move') { const [x, y] = argument.split(',').map(Number); await page.mouse.move(x, y); }
            else if (verb === 'press') await page.mouse.down();
            else if (verb === 'release') await page.mouse.up();
            else if (verb === 'key') await page.keyboard.press(argument);
            else if (verb === 'down') await page.keyboard.down(argument);
            else if (verb === 'up') await page.keyboard.up(argument);
            else if (verb === 'reload') await page.reload();
            else if (verb === 'shot') await capture(shot.replace(/\.png$/, `-${argument}.png`));
        }
    })();
    const limit = marker ? 120000 : seconds * 1000;
    const outcome = await Promise.race([done, new Promise(resolve => setTimeout(() => resolve(marker ? 'timeout' : 'done'), limit))]);
    await runActions;
    await new Promise(resolve => setTimeout(resolve, 500));
    console.log(`RESULT ${outcome}`);
    if (outcome !== 'done') status = 1;
} catch (error) {
    console.log(`RESULT timeout: ${error.message}`);
    status = 2;
}
if (!shotTaken) await capture(shot);
await browser.close();
server.close();
process.exit(status);
