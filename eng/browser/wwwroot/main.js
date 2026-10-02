// The default page for a CNA.NET game: the canvas is the one SDL renders to (#canvas), and the
// game's own Main runs as it would on a desktop. In a single-threaded bundle Game.Run returns at once
// and the run goes on in animation frames, so runMain() -- not run(), which exits the runtime when
// Main returns -- keeps the runtime alive.
import { dotnet } from './_framework/dotnet.js'

const canvas = document.getElementById('canvas');
canvas.focus();
// A threaded bundle (WasmEnableThreads) runs each thread on a web worker, and .NET 11 starts a
// thread only on a worker it loaded before Main: a Thread.Start that needs one created later never
// returns. A probe with no CNA in it starts 3 threads on the default pool of 7 and 12 on 16 -- the
// runtime keeps four -- and the Racing Game Kit's loading thread hung on the default. A
// single-threaded bundle has no pool and ignores the setting.
const { runMain } = await dotnet.withConfig({ pthreadPoolInitialSize: 16 }).withModuleConfig({ canvas }).create();
await runMain();
