// The default page for a CNA.NET game: the canvas is the one SDL renders to (#canvas), and the
// game's own Main runs as it would on a desktop. Game.Run returns at once in a browser and the run
// goes on in animation frames, so runMain() -- not run(), which exits the runtime when Main
// returns -- keeps the runtime alive.
import { dotnet } from './_framework/dotnet.js'

const canvas = document.getElementById('canvas');
canvas.focus();
const { runMain } = await dotnet.withModuleConfig({ canvas }).create();
await runMain();
