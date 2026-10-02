# CNA.NET compatibility campaign (started 2026-10-01)

Goal: existing XNA 4.0 C# source runs on CNA with no `.cs` changes -- same product category as
MonoGame/FNA. Source code, headers, tests, real builds and runtime behaviour are the authority;
`plan.md`, `NEXT.md`, `docs/native-behavior-blockers.md` and the sibling repos' reports are
historical evidence only. Cross-repository fixes (cna, sharp-runtime, cna-cs, cna-cs-template,
cna-cs-samples) are authorized; nothing is pushed.

Rules kept from the owner brief:

* A defect is fixed in the layer it lives in, with a regression test at the lowest layer that can
  see it plus the C# consumer that exposed it. No managed workaround that breaks native lifetimes,
  swallows errors or adds hidden global state.
* ABI admission stays an exact reviewed point list; never "anything newer".
* Original Microsoft sample `.cs` files stay byte-identical (hash-verified); adapt project glue,
  hosts and compatibility assemblies instead. Any unavoidable edit is recorded with an exact diff.
* Strict `Microsoft.Xna.*` facade carries no non-XNA API; phone/device APIs go to a separate opt-in
  assembly.
* Browser/Android are qualified only by real runs; Windows/macOS/iOS stay unqualified from Debian.
* Builds: shared ccache, existing build dirs, `-j8` max; GPU/window runs only through
  `cna/tools/platform/run_gpu_tests_private.sh` or a private display, never `:0`/`wayland-0`.

## Phases and tasks

Status: `todo`, `doing`, `done`, `blocked(<reason>)`.

### P1 -- current truth (done 2026-10-01)

| ID | Task | Status |
| --- | --- | --- |
| CSX-001 | Record HEADs, toolchains, build trees of all repos | done |
| CSX-002 | Regenerate import counts against current headers | done: 1002 imports, 958 present, 44 absent |
| CSX-003 | Managed test baseline | done: Framework 627/627, XnaCompat 225/225 |
| CSX-004 | Native integration baseline against current library | done: 0.21 binding refused by its own policy (as designed); after migration 200/200 |

### P2 -- ABI 0.21.0 -> 0.35.0 migration

| ID | Task | Status |
| --- | --- | --- |
| CSX-010 | Remove `CNA.Graphics.Experimental` (PostProcessChain, RenderTargetPool, HdrPassSettings), its 44 imports, `CnaPostProcessContext`, engine-layer helpers and their tests: CNA retired the native layer in ABI 0.30 (`MOD-RETIRE-1`), it is not XNA API | done |
| CSX-011 | `tools/abi-verify` against 0.35 headers: layouts, prototypes, callbacks, constants, negative controls | done |
| CSX-012 | Policy 0.35.0 (point matrix), fixtures, canaries, docs; retire 0.21.0 | done |
| CSX-013 | Fix stale tooling paths (`cnanext`, `sharp-runtimenext`, `../cna-samples`) | done (scripts, CI pins; paths.py already sibling-aware) |
| CSX-014 | Integration suite green against real current `libcna_c_api.so` (OPENGLES3 + compiled effects, Release `build-probe`) | done |

### P3 -- native defects exposed by C#

| ID | Task | Status |
| --- | --- | --- |
| CSX-020 | CNA-REPORT-004: component callbacks driven by `Game::Update/Draw/Initialize` run outside the C callback borrow scope, so `DrawableGameComponent.GraphicsDevice` cannot be borrowed there. Native repro, fix, C test, C# test, rerun samples | done natively + C# test (CNA bdf239360, CBIND-128); sample rows rerun under CSX-050 |
| CSX-021 | CNA-REPORT-002 (model/effect ownership): rerun blocked samples, close if fixed | done: CNA-REPORT-002 was cna-cs (5f6c212); 001 resolved in CNA 8c713f1d8; 003 not reproduced; blocker register re-measured |
| CSX-022 | Compiled XNA effects with `CNA_EASYGL_COMPILED_EFFECTS=ON`: verify from current tree | done: ColorReplacement renders its compiled ReplaceColor effect on build-probe (OPENGLES3, compiled effects ON) |

### P4 -- template

| ID | Task | Status |
| --- | --- | --- |
| CSX-030 | Template builds/runs against migrated binding; fresh `dotnet new` generation verified (dev + package) without hidden absolute paths | done: template ae6b906, cna-cs packaging (installed native dir); dev + package acceptance pass |
| CSX-031 | Platform layout decision (desktop/browser/android projects) | done: one game source set, one project per platform head -- the desktop project plus opt-in `Platforms/Browser` (Microsoft.NET.Sdk.WebAssembly, `eng/browser/CNA.Browser.targets`) and `Platforms/Android` (`eng/android/CNA.Android.targets`); the template ships them (development consumers), and the samples' generators produce the same shape from a desktop project |

### P5 -- XNA surface expansion

| ID | Task | Status |
| --- | --- | --- |
| CSX-040 | Inventory current CNA GamerServices/Guide/Avatar/Net C API vs XNA 4.0 metadata | done (fbff5e7): 426 routes bound, abi-verify clean |
| CSX-041 | `Microsoft.Xna.Framework.GamerServices` runtime profile (Gamer, SignedInGamer, Guide, GamerServicesComponent, profiles, achievements, leaderboards) | done: 55/75 of the GS/Net profile exact; rest is CSX-042/043 |
| CSX-042 | Avatar types (AvatarDescription, AvatarAnimation, AvatarRenderer, ...) | done: 58/75 exact, renderer reaches Ready and draws |
| CSX-043 | `Microsoft.Xna.Framework.Net` (NetworkSession, AvailableNetworkSession, NetworkGamer, LocalNetworkGamer, PacketReader/Writer) | done: GS/Net profile 75/75, SystemLink loopback measured |
| CSX-044 | Missing C API routes added to CNA with pure-C tests where native C++ already has behaviour | doing: packet-reader copy done (CNA 402c1aaa9, ABI 0.36.0); LeaderboardWriter session route and PropertyDictionary stream contents open (Windows XNA throws for the writer too) |
| CSX-045 | Separate opt-in phone compatibility assembly (`Microsoft.Devices` etc.) only where samples need it | done: `CNA.PhoneCompat` (Devices + Sensors.Accelerometer); 6 of 7 gallery phone samples compile unchanged; Phone.Shell/Notification for Yacht open |

### P6 -- samples (`cna-cs-samples`)

| ID | Task | Status |
| --- | --- | --- |
| CSX-050 | Requalify existing checked-in C# rows against migrated binding | done (cna-cs-samples bd9d069): 30/30 build Debug+Release, run, capture; every Escape row exits 0; 28 rows done; found CSX-083 and CSX-084 |
| CSX-051 | Generated inventory: gallery (`samples.libcna.com`) x C++ evidence (`/rv/tmp/samples`) x original source (`/rv/tmp/XNAGameStudio/Samples`) | done (cna-cs-samples 4ebd96c): 84 gallery samples |
| CSX-052 | Update obsolete cna-cs-samples policy (read-only CNA, stop-for-owner) | done (cna-cs-samples 26d06c1) |
| CSX-053.. | One task per eligible sample: unchanged source, XNB content, Debug/Release, run, controls, clean exit, pixel comparison | 83 of 84 rows ✅, CSSAMPLE-071 Yacht 🛑 owner decision (cna-cs-samples plan.md, 2026-10-01) |
| CSX-104 | Real XNA 4.0 games beyond the gallery, unchanged (cna-cs-samples `games/`) | done on the Linux desktop: 22 games run -- Speedy Blupi, Rookie Drivers, TIE Fighter Forever, Resonance, Microsoft's Solitaire and Moto Trial Racer, NePlus, the XNASidescroller, Virulent, Kosmic Warz, Dominó Tropical, A Princess' Request, Swf2XNA's My Big Head and Playing in Traffic, Escape From Enceladus (2026-10-02, after CNA FX-141, CSX-109 and CSX-110), __Defense (after CNA CBIND-145, CSX-111 and CSX-112), Missile Command, a Super Mario World demo, a Zelda clone and Bubble Bound play or reach their menus; HeliumBiker (Wii Remote), Zombie Smashers and Playing in Traffic (Xbox 360 gamepad) wait for the controller they read, as on Windows without one. Content their repositories ship, or built from their own content projects by XNA's BuildContent under Wine (songs and videos as labelled stand-ins); prebuilt XNA libraries run through CSX-099. Four XNA builds compared with XNA under Wine (title frames 8 px to 0.32%). Nineteen recorded with the reason outside XNA (Snails' published source builds on no platform; two need commercial fonts; one needs its defunct server) (Windows API, Windows Forms, Silverlight, files their repositories do not ship). Found CSX-105/106/107 and CNA Task 1120, CBIND-142. In a browser 14 of the 16 reach their title or play (NePlus and Princess Request after generator fixes; Resonance its menu, its levels load on a thread); HeliumBiker starts a thread (single-threaded WebAssembly) and Playing in Traffic plays a video (no video backend in the browser build). On Android 15 of the 16 run on the emulator (Playing in Traffic stops at the same refusal of its video) |

### P7 -- browser

| ID | Task | Status |
| --- | --- | --- |
| CSX-060 | Experiment: .NET wasm + `wasm-tools` workload + static CNA via `NativeFileReference` | done: .NET 11 (emscripten 6.0.3, libc++ 21) links CNA's WebGL2 archives; a C# XNA game draws in headless Chromium. .NET 10/8 pin emscripten 3.1.56/3.1.34, whose libc++ 17 has no `std::jthread` |
| CSX-061 | Frame-stepped game loop on browser event loop | done: `Game.Run` on the browser runs CNA's host-driven run (ABI 0.38.0) from `requestAnimationFrame`; an unchanged XNA `Main` (`using (game) game.Run();`) runs to `Disposed` in Chromium |
| CSX-062 | Real Chrome run of an unchanged XNA-style game: rendering, input, lifecycle, reload | done in headless Chromium: AimingSample unchanged, 0 px from its C++ port; keys, mouse, its Escape exit and a reload driven. Desktop Chrome with a GPU not run here |
| CSX-063 | Browser sample corpus | done: all 84 checked-in rows build and run in headless Chromium (cna-cs-samples `scripts/browser-requalify.sh`, CNA `ffb82bc0d`, 2026-10-02); 77 on the first pass, the other seven after CSX-108 (isolated storage), CNA CBIND-143 (avatar loader thread) and two generator fixes (files copied beside the game, Windows-spelled paths). Most pixel-identical to their desktop capture; the rest differ by animation or seeded randomness |

### P8 -- Android

| ID | Task | Status |
| --- | --- | --- |
| CSX-070 | Toolchain check (`~/Android/Sdk`, NDK, .NET android workload), per-ABI CNA build | done: NDK 29 builds CNA's C API for x86_64 (CBIND-136); `eng/android` runs an unchanged XNA `Main` on SDL's thread; AimingSample draws, takes touch and Back, survives pause/resume and relaunch on the emulator |
| CSX-071 | Android host + sample, emulator run: lifecycle, graphics recreation, touch, content, audio | done on the emulator: all 84 rows build, run and draw (cna-cs-samples `scripts/android-requalify.sh`, CNA `ffb82bc0d`, 2026-10-02); 78 end on one Back tap, six do not end on one key on the desktop either (their menus take more); pause/resume, relaunch, touch; audio not heard (`-no-audio`), no physical device |

### P5b -- behavioural gaps found on the way

| ID | Task | Status |
| --- | --- | --- |
| CSX-080 | Facade exception types: map native refusals to the XNA exception the same call throws | done 2026-10-02 for every boundary real code catches around: ABI 0.37.0 names the canonical exception (CNA e9dd5d879); GS/Net/Avatar/Guide, PhoneCompat, the audio/media/content boundaries and `new Game()` (NoSuitableGraphicsDeviceException, CNA CBIND-144) re-raise it. Checked against every `catch` of a specific exception type in the upstream samples and the real games (2026-10-02): the rest wrap managed calls that already throw XNA's type (`TitleContainer.OpenStream`'s FileNotFoundException, SpriteFont's ArgumentException for a character it lacks) or BCL calls. Other facade calls keep `CnaException`, carrying the canonical type, until a caller is found that catches them |
| CSX-082 | Callback exceptions unwind out of `Run`/`Components.Add` with their own type and stack | done (741e441) |
| CSX-081 | `GraphicsDeviceManager` default profile from the `Microsoft.Xna.Framework.RuntimeProfile` resource (XNA IL `ReadDefaultGraphicsProfile`), plus MSBuild glue embedding it from `<XnaProfile>` | done; samples' Directory.Build.targets import + per-sample XnaProfile is part of CSX-050 |
| CSX-083 | Device state cache coherent with what native SpriteBatch applies; BlendFactor/MultiSampleMask/ReferenceStencil dirty the state as XNA's setters do | done |
| CSX-084 | SIGTERM/SIGINT during `Run` end the game through its own exit instead of the runtime's `exit()` racing the game thread | done |
| CSX-085 | A `CNA_Handle` is held as the 64-bit value it is, not narrowed to pointer width (WebAssembly, any 32-bit target) | done |
| CSX-086 | `Game` lifecycle in XNA's order: `BeginRun` after Initialize/LoadContent, `EndRun` after `Exiting`, `Disposed` raised | done |
| CSX-087 | Managed content paths resolve against the title directory, as XNA's `ContentManager.OpenStream` does through `TitleContainer`, not the working directory | done; the native half is CNA CBIND-140 (9976f4909): SimpleAnimation runs from an unrelated directory |
| CSX-088 | Components initialized, content included, inside `base.Initialize()` in XNA's order, once | done |
| CSX-089 | A `Model` whose tag is of the game's own type loads through the game's reader | done |
| CSX-090 | `base.Update`/`base.Draw` update and draw the components at the call, once per frame | done |
| CSX-091 | `Color.Transparent` is XNA 4.0's transparent black | done |
| CSX-092 | The managed model path reads every stock effect XNA writes into a model | done |
| CSX-093 | A model tag holds XNA's types, and `object` has an element reader | done |
| CSX-094 | A Windows Phone title's `IsFullScreen` is its status bar, not a desktop display mode | done |
| CSX-095 | A Windows Phone title's player one is the phone: connected, Back on Escape | done |
| CSX-096 | File paths given to XNA's file-taking APIs resolve as on Windows (separators, case) | done |
| CSX-097 | `Microsoft.Phone.Shell.PhoneApplicationService` in CNA.PhoneCompat: Launching after LoadContent, Closing at exit | done |
| CSX-098 | A Windows Phone title off a phone gets the mouse as a finger, as in the emulator | done |
| CSX-099 | A library compiled against XNA 4.0 runs unchanged: XNA-named forwarders, no Microsoft key | done |
| CSX-100 | A game's own worker thread loads content, creates resources and moves their data, as XNA allowed | done |
| CSX-101 | ABI 0.39.0: the rest of a loading thread's calls run on the game thread (CNA CBIND-141) | done |
| CSX-102 | A content reader is found by its assembly's simple name, as .NET Framework bound an unsigned assembly | done |
| CSX-103 | `PhoneApplicationService.StartupMode`: a desktop process is always launched | done |
| CSX-105 | A song whose `.wma` was converted plays from the `.ogg`/`.oga`/`.qoa` beside it, as in CNA and FNA | done |
| CSX-106 | `ActivatedEventArgs.IsApplicationInstancePreserved` (Windows Phone 7.5): a desktop process always is | done |
| CSX-107 | A stock effect in `SpriteBatch.Begin` places the sprites in 3D, as XNA does (CNA Task 1120) | done |
| CSX-108 | `System.IO.IsolatedStorage` in a browser build (CNA.BrowserCompat), where .NET ships only a PlatformNotSupported stub | done |
| CSX-109 | A storage device several worker threads share works from each of them, as XNA's did | done |
| CSX-110 | `List<T>.ForEach` as the .NET Framework 4.0 that XNA games target ran it, by compile-time interception | done |
| CSX-111 | ABI 0.40.0: the graphics adapters answer in a game's constructor (CNA CBIND-145) | done |
| CSX-112 | `GraphicsDeviceManager.GraphicsDevice` (and `Game.GraphicsDevice` with a device service) is null until the device exists, as in XNA | done |

### P9 -- portability

Windows/macOS/iOS: architecture only (resolver keeps `.dylib`/`.dll`; iOS planned as static
`__Internal`). Not qualified.

## Ledger

Newest first. Each entry: repos+HEAD, reproduced, root cause, files, tests, commands, results.

### 2026-10-02 -- CSX-112: no device yet is null, not an exception

gamealgorithms/defense, past CSX-111, died next in its constructor: `GraphicsManager.SetResolutionToCurrent`
tests `if (m_Graphics.GraphicsDevice != null)` before applying the resolution, and CNA.NET threw
"The graphics device is not available until the game has initialized". XNA's IL: the manager's
getter returns its device field, null until `Run` creates it; `Game.GraphicsDevice` returns its
`IGraphicsDeviceService`'s device the same way and throws only when there is no such service
("This property requires a graphics device service in the game service container."). Both now do;
a game with no service still gets its device once it runs, because CNA creates one for every game.
The constructor case in `CompatGame_ReadsTheGraphicsAdaptersInItsConstructor` fails without the
change. Integration 231/231, XnaCompat 290/290, api-compat 0 diagnostics. Defense then shows its menu.

### 2026-10-02 -- CSX-111: the graphics adapters in a game's constructor (ABI 0.40.0)

gamealgorithms/defense died in its `Game` constructor: `Adapters failed with native result
InvalidState: The graphics device may be borrowed only during a game lifecycle callback`. It reads
`GraphicsAdapter.DefaultAdapter.CurrentDisplayMode` there to size its back buffer -- XNA's usual way,
and XNA answers it anywhere -- and CNA.NET borrowed the game's device for the adapter routes, which
CNA lends only inside a callback. CNA CBIND-145 (C ABI 0.40.0, `22b30b30e`) lets the twelve
`cna_graphics_adapter_*` routes take the active game's own handle; CNA.Framework's `GraphicsAdapter`
now passes it. Admission as for 0.39.0: baselinediff 0.39.0 -> 0.40.0 1413 consumed exports, 0
absent, 0 changed, 0 added, 0 breaking; the matrix accepts exactly 0.40.0 and retires 0.39.0, whose
exports are identical, so `retired-0.39.0` is refused on the version rule alone; fixtures move to
exact-0.40.0, retired-0.39.0 and unreviewed-0.41.0 (the two shape fixtures too, which otherwise
failed for their version first). CI pins `22b30b30e`, previous accepted `0a57ccad2`.
`tools/abi-verify` had stopped at `CNA_STARTUP_MODE_LAUNCH undeclared` since CSX-103: it checked every
public phone enum, and `StartupMode` never reaches native; its scope is now the `Microsoft.Devices`
enums. Results: abi-verify 1137 values, 23 callbacks, 604 constants, 0 mismatches; fixtures passed;
`CompatLayerIntegrationTests.CompatGame_ReadsTheGraphicsAdaptersInItsConstructor` (fails without the
change); integration 231/231, GamerServices 24/24 (now with isolated XDG directories), framework
645/645, XnaCompat 290/290, api-compat 0 diagnostics; package acceptance passed on 0.40.0.

### 2026-10-02 -- CSX-110: List<T>.ForEach as .NET Framework 4.0 ran it

escape-from-enceladus died the moment its first room started: `InvalidOperationException: Collection
was modified` from `_activeEvents.ForEach(e => e.Update(gameTime))`, whose events activate the next
event. .NET Framework 4.0's `List<T>.ForEach` re-reads the count at each step and never throws; 4.5
added the throw only for applications that target 4.5 or later (`BinaryCompatibility`), and .NET Core
throws for every one. XNA 4.0 games target 4.0, so the game ran on Windows. New
`src/CNA.XnaCompat.Generators` (netstandard2.0, Roslyn 4.11) intercepts each of the game's own
`List<T>.ForEach` calls (C# interceptors, namespace `CNA.XnaCompat.NetFx40`, generic so calls inside
generic code are covered) with the 4.0 loop -- `ArgumentNullException("match")` included. Nothing
outside the game's assembly changes and its source is not touched. CNA.XnaCompat.targets enables it
(both compilers' properties) and references the generator for project consumers; the CNA.XnaCompat
package carries it under `analyzers/dotnet/cs`. `<CnaNetFx40ListForEach>false</CnaNetFx40ListForEach>`
opts out. `NetFx40ListForEachTests` (append, remove, null, generic method) pass, and fail with the
opt-out; XnaCompat 290/290; package acceptance passed on CNA `46231e857` (and a package consumer printed
`1,2,3,4`); the .NET 11 SDK's compiler runs it too. The game then plays its first room.

### 2026-10-02 -- CSX-109: one storage device, several worker threads

zachmu/escape-from-enceladus's title screen loads its three save slots at once, each on a thread of
its own sharing one static StorageDevice: the first thread to need it selects it under a lock, the
others open containers on it. Two of the three died with `OpenContainer failed with native result
Thread`: a CNA handle answers only the thread that created it, and the device was created by
whichever worker selected it. CNA serves another thread's calls on the game thread (CBIND-141), but
this handle was not the game thread's. `StorageDevice.ShowSelector` (all four overloads, so every
`BeginShowSelector`) now selects on the game thread (`GameThread`); every later call on the device,
and on the containers and streams it opens, is then served there. The callback of a
`BeginShowSelector` still runs on the caller's thread. A sequential version of the test passed even
without the fix: a new thread can inherit a finished one's native thread id. The concurrent
`CompatWorkerThreadTests.WorkerThreads_ShareOneStorageDevice_WhileTheGameRuns` (three workers, each
writes, reads back and deletes its own file) fails without the fix and passes with it. Integration
230/230 twice, Framework 644/644. The game then reaches its title screen with every slot read.

### 2026-10-02 -- CSX-080: a game with no graphics device, in XNA's exception

Reproduced on a private Xvfb without GLX: an unchanged gallery game (Platformer) died with
`CNA.CnaException: cna_game_create failed with native result Platform: ... CreateWindow failed: GLX
is not supported`, and with no display at all with `AcquireSubsystem(Video) failed`. XNA throws
`NoSuitableGraphicsDeviceException` there -- `GraphicsDeviceManager.CreateDevice` (IL) wraps any
failure to create the device in it -- and the Racing Game Kit's `Program` catches exactly that to tell
the player the machine cannot run it. CNA creates the device in `Game`'s constructor, so CNA
`a71cc2415` (CBIND-144) wraps there, and the XNA `Game` constructor now re-raises the canonical
exception (`XnaExceptions.Guard`). `CompatGameCreationTests` runs the integration assembly again as a
child process (`Program.cs`; the test host never calls it) with an unreachable X server and
compositor, so no window can appear anywhere: `Microsoft.Xna.Framework.Graphics.NoSuitableGraphicsDeviceException`,
"Unable to create the graphics device. ..."; before the guard `CNA.CnaException`. Integration
229/229, XnaCompat 286/286. The protected native library CI uses must be at CNA `a71cc2415` or later
for that test; no ABI change.

### 2026-10-02 -- CSX-104: the games that cannot run here, compiled

Ten real XNA 4.0 codebases that cannot run here (Windows Forms or Win32, Silverlight, content or
libraries their repositories do not ship) were compiled against CNA.NET `024f2b2` to look for XNA
API they use that CNA.NET lacks: each original `.csproj`'s own Compile items, DefineConstants and
HintPath libraries, its project references mapped one to one, and an empty `System.Windows.Forms`
namespace so that Roslyn binds method bodies past the using directive. Racing Game Kit, Mannux,
infinecraft (Core + BlockRLH), WPDrumkit and Flux (FluxEngine + its prebuilt Farseer) compile but
for Windows Forms and `System.Drawing`. DiseasedToast stops at Nuclex.Input (not in its repository;
its TiledLib is a content-pipeline importer), EvoNet at Windows Forms and YamlDotNet, WP.NuPogodi at
Silverlight, Sleepwalker at four calls into an older Swf2XNA runtime than the one its repository
ships. Minor Destruction (with Lidgren, MiningGameServer and its shipped GeeUI.dll) fails only at
`BitConverter.GetBytes(sbyte)`, which .NET 7's implicit `sbyte`-to-`Half` conversion made
ambiguous -- a change in .NET, not in XNA's API. No XNA type or member was missing.

### 2026-10-02 -- CSX-031: the template in a browser and on Android

cna-cs-template `2a76388`: `Platforms/Browser` and `Platforms/Android` compile the template's own
`.cs` files and `Content/`. To serve a hand-written project rather than only the generated ones,
`CNA.Browser.targets` (`3c8e556`) now references CNA.BrowserCompat as a project, supplies the page
and start-up script when a project has none, and stages a game's Content (`CnaGameContentDirectory`)
under the project's own wwwroot: a file linked in from elsewhere conflicts with itself in .NET 11's
ComputeWasmVfs. Evidence: the repository's heads and a `dotnet new cna-game` game's heads both run --
the cube with its logo in headless Chromium and on the emulator (Content extracted); the generated
desktop project still builds; `verify-template.sh` passes in both modes; package acceptance passes
on CNA `ffb82bc0d` (three packages, 60/600 frames, no native environment); a Package-mode game is
generated without `Platforms/`.

### 2026-10-02 -- Browser and Android corpora on CNA ffb82bc0d

Browser (`scripts/browser-requalify.sh`, CNA.NET from `11e5bd3` to `7ae5f08`, headless Chromium,
SwiftShader WebGL2): 84 rows, two runs of 42 (`/rv/tmp/cs-samples/browser-requal-csx102-{a,b}`), 77
passed. The seven others, each fixed where it lived and rerun (`browser-requal-csx108`,
`browser-requal-winpath`): four Windows Phone rows needed isolated storage (CSX-108);
InverseKinematics's avatar loader started a thread (CNA CBIND-143); Spacewar's settings.xml was not in
the page's file system and NetRumble's Windows-spelled directory had no link there (cna-cs-samples
generator). 84/84. A capture artifact was found on the way: a canvas larger than the 1280x800 viewport
came out tiled once its game asked for fullscreen; the runner's viewport is 1920x1080 (`bb48a05`).

Android (`scripts/android-requalify.sh`, emulator x86_64, CNA C API staged at `ffb82bc0d`): 84 rows,
77 passed; Spacewar (settings.xml) and NetRumble (entry point in a private nested class) fixed in the
generator and rerun. All 84 build, run and draw; 78 end on one Back tap, and the six that do not
(ReachGraphicsDemo, GameStateManagement, ClientServerSample, ShipGame, RolePlayingGame, NetRumble) do
not end on one key on the desktop either.

The real games in a browser (`scripts/browser-sample.sh games/<Game>`): 14 of 16 reach their title or
play; HeliumBiker starts a thread and Playing in Traffic plays a video, neither available in this
browser build.

Desktop, the same day on the same CNA (`scripts/requalify.sh`, `/rv/tmp/cs-samples/requal-20261002`):
84/84 build (Debug and Release) and run, and every row's build, run and exit result is the
2026-10-01 baseline's (`requal-20261001-csx098`): Task 1120, CBIND-142 and CBIND-143 changed no
gallery row. The six that do not exit are the six above.

### 2026-10-02 -- CSX-108: isolated storage in a browser

The browser corpus (84 rows, CNA `1c2923efd`, CNA.NET `11e5bd3`+) passed 77; UISample, MarbleMaze,
HoneycombRush and NinjAcademy failed `IsolatedStorage_PlatformNotSupported`: .NET 11's browser-wasm
runtime pack ships System.IO.IsolatedStorage as a stub that throws from every store, and Windows Phone
games save through it (UISample's ScreenManager reads its saved screens at startup). New assembly
`src/CNA.BrowserCompat` implements System.IO.IsolatedStorage (IsolatedStorageFile, IsolatedStorageFileStream,
IsolatedStorageException, IsolatedStorageScope, IsolatedStorage) over the browser's file system, under its
own name; `eng/browser/CNA.Browser.targets` drops the framework's reference and references it
(`build/CNA.BrowserCompat.targets`), so nothing takes the framework's identity. Its behaviour is .NET's as
measured on the desktop (a probe: unlimited quota, no used size, a silent delete of a missing file,
IsolatedStorageException wrapping the IO failure, a closed store refused) except that a backslash
separates directories, as on Windows Phone (.NET on Unix makes `Dir\a.dat` one file name).
`tests/CNA.BrowserCompat.Tests` 5/5. The four rows then pass in headless Chromium: three
pixel-identical to their desktop captures, NinjAcademy 45 px. The store lasts as long as the page.

### 2026-10-02 -- CSX-107: sprites in a 3D world

Kosmic Warz played with a black sky: its starfield is a DPSF `Sprite3DBillboard` particle system, which
draws through `SpriteBatch.Begin(..., AlphaTestEffect)` with each particle's view-space position as
`(x, y)` and its depth as `layerDepth` -- XNA 4.0's way to place sprites in 3D, as 3D text with
`BasicEffect` is. XNA applies the custom effect's pass after the sprite effect's, so the stock effect's
vertex shader places the sprite; CNA kept its 2D sprite projection, and the sprites were clipped.
Fixed in CNA (Task 1120, `7661f6981`): such a batch is drawn as XNA draws it, through the device.
`CompatLayerIntegrationTests.CompatSpriteBatch_AStockEffectPlacesTheSprites` reads it back (a one-unit
sprite through an orthographic [-1, 1] projection covers the top-right quadrant; before the fix
nothing). Integration 228/228 on CNA `7661f6981`. Kosmic Warz now draws its stars.

### 2026-10-02 -- CSX-106: Kosmic Warz's activation handler

ActiveNick/KosmicWarz-WP (a Windows Phone 7.5 shooter) did not compile: its `Activated` handler passes
`e.IsApplicationInstancePreserved` to its screen manager. CNA.PhoneCompat's `ActivatedEventArgs` now
has it, answering `true` -- a desktop process outlives anything that would deactivate it, and
`Activated` is never raised here. `PhoneApplicationServiceTests.Activated_InstanceIsPreserved`.

### 2026-10-02 -- CSX-105: an XNA game's music

XNA's pipeline writes every song as Windows Media Audio and the `.xnb` names that `.wma`; nothing in
CNA decodes WMA. FNA's `SongReader` and CNA's own therefore drop the reference's last four characters
and play the `.ogg`/`.oga`/`.qoa` beside it, so a port converts its music and keeps its `.xnb`. CNA.NET's
managed reader passed the `.wma` straight through: with a converted file beside it the game was still
silent, and without the `.wma` the load failed. Found with Mavinkea/XNASidescroller (its shipped
`nbinstrumental.wma`): SDL's disk audio driver recorded 6.9 s of zeros. `SongContentReader.PlayablePath`
now probes the same way; `SongWireFormatTests.WmaReference_PlaysFromTheConvertedFileBesideIt` fails
without it. Integration 226/226 (Debug). With `nbinstrumental.ogg` converted beside it the game's
music plays (99.4% non-zero samples, peak 6914 at the game's own volume 0.4).

### 2026-10-01 -- CSX-103: Microsoft's XNA Solitaire

`microsoft/solitaire-wp` (XNASolitaire, the Windows Phone 7 game Microsoft published with XNA Game
Studio 4.0) did not compile: it asks `PhoneApplicationService.Current.StartupMode` whether to restore
a tombstoned game. CNA.PhoneCompat now has the `StartupMode` enum and the property, which answers
`Launch` -- nothing tombstones a desktop process. `PhoneApplicationServiceTests.StartupMode_IsLaunch`.
With its 56 textures built by XNA's BuildContent (WindowsPhone/Reach) the game deals a Klondike
layout and a tap on the stock turns a card (mouse as finger, CSX-098). cna-cs-samples `games/Solitaire`.

### 2026-10-01 -- CSX-102: a game's own content reader in a browser

HeightmapCollision failed in the browser corpus: "Could not find ContentTypeReader Type
'HeightmapCollision.HeightMapInfoReader, HeightmapCollision, Version=1.0.0.0, Culture=neutral'",
the first gallery row to run a reader of its own in a browser. A browser probe
(`build-probe/browser-reader-probe`) showed the class loads, and that the browser runtime's
`Type.GetType` refuses exactly that spelling -- `Culture=neutral` without `PublicKeyToken=null` --
while accepting it with the token or without the culture; CoreCLR accepts all of them. XNA's
pipeline writes it for every unsigned game, and .NET Framework bound an unsigned assembly by simple
name alone, so `ContentTypeReaderManager` now resolves reader assemblies by simple name (loaded
first), for the reader and its type arguments, and its error carries the runtime's own reason.
`ContentReaderCompatibilityTests.Load_ReaderNamedWithAnotherVersionAndNoPublicKeyToken_ResolvesBySimpleName`
pins the policy on the desktop (where it passed before too); the browser row is the regression
evidence: HeightmapCollision draws its terrain and ball. XnaCompat 286.

### 2026-10-01 -- CSX-099: XNA-compiled libraries run unchanged

resonance-game ships BEPUphysics v1.0.0 as binaries compiled against XNA 4.0, so its references
name `Microsoft.Xna.Framework, Version=4.0.0.0, PublicKeyToken=842cf8be1de50553`. `src/XnaAssemblies`
now holds XNA's ten runtime assemblies by simple name and version, each only type forwards to
CNA.XnaCompat -- all 325 public XNA 4.0 types; `tools/xna-assemblies` generates them from XNA's
reference assemblies and CI checks them (the counts match FNA's `abi/` exactly). They carry no public
key, keeping docs/xna-compatibility.md's rule that CNA.NET does not take Microsoft's identity. The
runtime binds by name regardless of the token (measured); the C# compiler does not, so
`CNA.XnaCompat.targets` gains `CnaRetargetXnaReferences`: a referenced library that names XNA by its
token is built against a copy without the token, and an x86-only pure-IL library (the XNA Game
Library default, which .NET 8 x64 refuses outright) is marked loadable in that copy. The task reads
ECMA-335 tables itself because an inline task compiles against netstandard. `XnaBinaryLibraryTests`
run an x86 library compiled with XNA Game Studio 4.0's own reference assemblies (math, a vertex
type implementing `IVertexType`, a `ContentTypeReader<T>` loading an XNB); before, the same binary
failed all four with BadImageFormatException. resonance-game plays with its prebuilt Windows Phone
BEPUphysics (its x86 build lays out a struct with an object field at offset 20, which no 64-bit
runtime loads). XnaCompat 285, Framework 644, Integration 224.

### 2026-10-01 -- CSX-101: the rest of a loading thread, through CNA

CSX-100 moved whole operations to the game thread; resonance-game's loading thread then read
`GraphicsDevice.Viewport`, and after that sets effect matrices and lights -- a long tail of
hundreds of routes. CNA CBIND-141 (C ABI 0.39.0, `0a57ccad2`) adds
`cna_game_set_foreign_thread_calls_ext`: a call that a handle refused only because another thread
created it is queued and run on the game thread inside its next Update/Draw callback scope. The
XNA facade turns it on for every game; CNA.Framework's own API keeps the native default. Device
queries (`Viewport`, `PresentationParameters`, `Adapter`, `DisplayMode`, ...) still take one managed
hop each, because the device CNA lends lasts one callback scope and resolving it and reading through
it must not be two queued calls. Admission as for 0.38.0: baselinediff 0.38.0 -> 0.39.0 1412
consumed, 0 absent/changed, 1 added, 0 breaking; abi-verify 1413/1413 prototypes, 1137 values, 23
callbacks, 604 constants, 0 mismatches; fixtures 2 accepted / 10 rejected (`retired-0.38.0`,
`unreviewed-0.40.0`). `CompatWorkerThreadTests` now also reads the viewport and presentation
parameters and sets matrices, default lighting and a light from the worker. Framework 644,
XnaCompat 285, Integration 224, GamerServices 24. Found on the way and fixed in their own commits:
CSX-098 had moved neither the import tripwire nor the ABI fixture inventory. resonance-game's
loading thread now runs to its first shader, which CNA's MojoShader path refuses (next).

### 2026-10-01 -- CSX-100: loading on a worker thread

resonance-game (`cna-cs-samples/games`, real game) loads its level on its own `Thread` while the
loading screen draws, as XNA 4.0 allowed. On CNA.NET the first `Content.Load` from that thread
failed with `CNA_RESULT_THREAD`: every CNA handle belongs to the thread that created the game, by
the C ABI's design. CNA.Framework's new `GameThread` runs such work on the game thread: the caller
blocks, the game thread runs queued calls at the start of each Update and Draw (draining for up to
8 ms while the worker keeps asking), and calls still waiting when the game is disposed fail. The
facade sends whole operations through it -- `ContentManager.Load`, every resource constructor and
the reads its facade constructor makes, `SetData`/`GetData`, `Texture2D.FromStream`/`SaveAs*`,
`Effect.Clone` -- and CNA.Framework's stock-effect property funnels, so the managed half of each
operation also runs on the game thread and CNA.NET's caches see one thread. On the game thread the
guards allocate nothing (static lambdas with a state tuple). `CompatWorkerThreadTests` does all of
it from a worker while the game runs. Integration 224, Framework 643, XnaCompat 285. Not covered
yet: the long tail (device getters such as `Viewport`, effect matrices, `EffectParameter`) --
resonance-game's next call is `GraphicsDevice.Viewport`; see CBIND-141.

### 2026-10-01 -- CSX-098: the mouse is the finger for a phone title off a phone

The phone rows qualified on first frames and Escape, but their touch-only controls were dead on a
desktop: the facade never asked for CNA's mouse-as-touch bridge, which is off by default to keep
XNA's and FNA's behaviour. The phone emulator made the mouse the finger, and the C++ ports of these
samples opt into the same bridge (at the owner's request, 2026-09-27). CNA.Interop now binds
`cna_touch_panel_{get,set}_mouse_touch_emulation_enabled_ext`, CNA.Framework exposes them
internally, and a phone title (`PhoneTitle`) turns the bridge on at its first frame; a Windows title
keeps the default. abi-verify: 1412 imports, 0 mismatches. `CompatPhoneTouchTests` (on for a phone
title, off for a Windows one). DynamicMenu: a mouse tap on "Page 2" now shows page 2, 0 px from both
the C++ campaign's frame and the original XNA game's. Integration 223.

### 2026-10-01 -- CSX-097: a phone game's application lifetime, off the phone

cna-cs-samples NinjAcademy (CSSAMPLE-065) did not compile: it subscribes to
`PhoneApplicationService.Current.Launching/Activated/Deactivated` in its constructor and keeps its
tombstone in `State`. A Windows Phone SDK type, so it goes into the opt-in CNA.PhoneCompat, not the
facade: `Microsoft.Phone.Shell.PhoneApplicationService` (Current, State, the four lifetime events)
and their event-args types, nothing else. Launching is raised once the game runs -- after
`Initialize` and `LoadContent`, before the first `Update`, because the sample's handler plays music
through the AudioManager it creates in `Initialize` -- through an internal `Game.Started` the facade
grants CNA.PhoneCompat; Closing on the game's `Exiting`. Nothing tombstones a desktop process, so
Activated/Deactivated are never raised. `PhoneApplicationServiceTests` (order of Initialize,
LoadContent, Launching, Update, and Closing last; one State per process). api-compat 256/256, 0
diagnostics. NinjAcademy: 0 px from the C++ campaign's menu frame, exits 0 on Escape (its Back).
Integration 222.

### 2026-10-01 -- CSX-096: XNA's file-taking APIs read paths as Windows did

Two of the newly eligible games (cna-cs-samples CSSAMPLE-066, -070) died before their first frame:
RolePlayingGame hands `AudioEngine` `Content\Audio\RpgAudio.xgs`, ShipGame hands it
`content/sounds/sounds.xgs` for `Content/Sounds/sounds.xgs`. Both are right on Windows, where XNA's
engine opened them with Win32. `XnaContentPath.ToHostPath` turns `\` into the host separator and,
when the path does not exist as written, matches each segment ignoring case (ordinal-first on a
tie, as the existing file-name matcher); a path matching nothing comes back separator-normalized
for the caller's own error. `AudioEngine` (still `Path.GetFullPath`, as XNA's IL), `WaveBank`,
`SoundBank` and `TitleContainer.OpenStream` use it. Content asset names already resolved this way.
`XnaContentPathHostTests` (5). Not reachable from the binding: a game handing such a path to the BCL
itself (NetRumble's `new DirectoryInfo(Content.RootDirectory + @"\audio\wav")`) -- that row carries
a recorded source deviation. Unit 281 + 643, integration 220.

### 2026-10-01 -- CSX-095: a phone title's player one is the phone

cna-cs-samples UISample (CSSAMPLE-082) could not be left: its main menu exits on
`IsNewButtonPress(Buttons.Back)` only, the phone's hardware Back button, and a desktop has no pad.
Nine other phone rows likewise test only `Buttons.Back` and answered no key at all. On Windows
Phone, `GamePad.GetState(PlayerIndex.One)` is the phone itself -- always connected, Back being the
hardware button. For a phone title off a phone (`PhoneTitle`, the RuntimeProfile check CSX-094
introduced, now set by each `Game`), player one reads as connected and Escape as Back, merged into a
real pad's state when one is present. Escape is the substitute the owner chose on 2026-09-27 for
the C++ ports of these samples, which edited the games to get it; here the games are unchanged.
Android and iOS are untouched (CNA CBIND-137 latches the system Back there). Unit:
`PhoneTitleTests` (4). All ten phone rows now exit 0 on Escape; UISample's requalify exit check
passes. Unit 281 + 638, integration 220.

### 2026-10-01 -- CSX-094: a phone title's full screen stays in the game on a desktop

The full-corpus requalify after CSX-093 (67/67 pass, `/rv/tmp/cs-samples/requal-20261001-csx093`)
captured SoccerPitch (CSSAMPLE-073) black: the game sets `graphics.IsFullScreen = true`, CNA asked
SDL for a 480x800 full-screen mode, and SDL gave up ("no window becoming fullscreen; reverting");
the capture caught the window mid-switch. On Windows Phone that property hides the status bar and
never changes the display mode, so the request was wrong on any desktop, not only on a bare Xvfb.
The facade now reads the platform from the game's `Microsoft.Xna.Framework.RuntimeProfile`
(`WindowsPhone.v4.0.<profile>`, as XNA's build writes it): a phone title off a phone keeps
`IsFullScreen`/`ToggleFullScreen` as its own state and runs in its window, as in the emulator;
Android and iOS keep forwarding it (their full screen is the phone's). Unit:
`PhoneTitle_KeepsFullScreenInTheGame_OnlyOffAPhone`; integration:
`CompatPhoneFullScreenTests` (reads back true, presents windowed). The ten phone rows rerun with no
full-screen errors and titled windows; their frames match their earlier evidence. Unit 277 + 638,
integration 220.

### 2026-10-01 -- CSX-093: model tags hold XNA's types

cna-cs-samples TrianglePicking (CSSAMPLE-048) died in its first `Update`: "Unable to cast object of
type 'CNA.BoundingSphere' to type 'Microsoft.Xna.Framework.BoundingSphere'". Its processor tags each
model with a `Dictionary<string, object>` holding a `BoundingSphere` and a `Vector3[]`, all built-in
readers, so the facade took CNA.Framework's parser -- which builds math values as `CNA.*` types,
right for CNA's API -- and handed its tag object to the game unchanged. The facade now checks the
parsed model's, meshes' and parts' tags: anything typed from CNA.Framework (directly, as an array
or generic element, or inside a collection) sends the model through the managed `ContentReader`,
whose readers construct XNA's types (CSX-089's path). That path then needed XNA's `ObjectReader`:
XNA's `ContentTypeReaderManager` starts every table with a reader for `System.Object` (IL: the
static constructor registers it; its `Read` throws, since a reference-typed value is always read
through its own type id), and `DictionaryReader<string, object>` asks for it. Both in
`CompatModelTagTests.Model_WhoseTagHoldsXnaMathValues_ReturnsThemAsXnaTypes` (the real asset's
reader names; fails without the change). TrianglePicking: 1 px from its C++ port. Unit 272 + 638,
integration 219.

### 2026-10-01 -- CSX-092: the managed model path reads the skinned and other stock effects

cna-cs-samples SkinningSample (CSSAMPLE-054) failed to load `dude`: "Could not find
ContentTypeReader Type 'Microsoft.Xna.Framework.Content.SkinnedEffectReader'". The model's tag is
the game's own `SkinningData`, so since CSX-089 it loads through the facade's `ContentReader`
protocol, whose table had `BasicEffectReader` but none of the other stock effect readers XNA's
pipeline writes into a model. Added `SkinnedEffectReader`, `AlphaTestEffectReader`,
`DualTextureEffectReader` and `EnvironmentMapEffectReader`, each in the field order of its XNA 4.0
reader IL (monodis of Microsoft.Xna.Framework.Graphics.dll). Five corpus assets name
`SkinnedEffectReader` (the dude models, `DudeWalk`); CNA.Framework's own reader note said none did,
and now says where they go. `CompatStockEffectReaderTests` writes all four with distinct values per
field (fails without the change on the first reader). SkinningSample now runs and animates; its
7.78% from the C++ port is the animation phase. Integration 218/218.

### 2026-10-01 -- CSX-071: every row on the Android emulator

cna-cs-samples `scripts/android-requalify.sh` builds each manifest row as an app, runs it on the
emulator (x86_64, read-only and wiped per boot, `swiftshader_indirect`), taps Back, and measures the
game's frame against the desktop capture. All 34 rows pass: built, ran without a managed exception
or native crash, drew, and ended on one Back tap. The table is
`/rv/tmp/cs-samples/android-requal-20261001/android-requalification.md`; the frame differences
(4-28% for most) are the 2.25x scale and the status bar drawn over a windowed game, and are
measurements, not verdicts.

What the corpus needed, each fixed where it lives:

* `eng/android`: an APK keeps the title's files as assets, which CNA's native loaders read but
  `System.IO` cannot, so XNA's managed readers, `TitleContainer` and a game's own file reads found
  nothing (ContentManifestExtensions). `CnaGameApplication` copies the title asset directories
  (`Content` by default) into the title directory once per installed APK, on SDL's thread, and makes
  it the working directory as Windows does for a game it starts (XNA's `AudioEngine` resolves its
  settings file against it).
* CSX-087: managed content paths resolve against the title directory (ColorReplacement's model).
* CNA CBIND-138: a portrait game runs in portrait (SnowShovel, AimingSample's declared `Portrait`).
* CNA CBIND-139: every `BasicEffect` model drew as scattered triangles on the emulator, whose GLES
  encoder answers vertex attribute queries from stale state; EasyGL rebases the indices on the CPU
  there.
* Test harness: `-wipe-data` (the AVD's userdata left less than Android's install threshold), the
  app uninstalled after each row, and immersive mode pre-confirmed -- its one-time explanation takes
  the Back key from a full-screen phone game.

Not covered: audio (the emulator runs `-no-audio`), arm64-v8a, a physical device, and a GL context
actually lost while paused (SDL kept it here).

### 2026-10-01 -- CSX-091: Color.Transparent is transparent black

DistortionSample (CSSAMPLE-032) still smeared its scene after CSX-090. Its map view (B) showed the
cause: cleared with `Color.Transparent`, the background was white here and black in the C++ port.
The shader treats any non-zero texel as a displacement, so the whole scene moved by half a texture.
XNA 4.0's getter is `new Color(0u)` (XNA IL) -- transparent white was XNA 3.1's -- and both
CNA.Framework and the facade defined (255, 255, 255, 0), with a test pinning it and a comment
claiming XNA. Corrected in both, the test now asserts packed value 0 for both types. DistortionSample:
0 px from its C++ port. Unit 272 + 638, integration 217.

### 2026-10-01 -- CSX-090: components update and draw at base.Update and base.Draw

Found with cna-cs-samples DistortionSample (CSSAMPLE-032): its `Draw` sets the scene target, draws
the background, calls `base.Draw` -- where its distortion component composites the scene -- and then
draws its HUD. Here the HUD was missing and the scene smeared: the facade's `base.Update` and
`base.Draw` were empty, and CNA's own `Game::Update`/`Game::Draw` ran the components after the
game's override returned, so they drew over the HUD out of order. XNA's `Game.Update`/`Game.Draw`
run the enabled/visible components in `UpdateOrder`/`DrawOrder` at the call (XNA IL). The facade now
does that over a stable-sorted copy (reused lists; no per-frame allocation) and marks the frame, so
CNA's pass through the adapters is a no-op; a game that never calls `base.Update` keeps CNA's pass,
as before. `CompatComponentPassTests`: a component's count rises inside `base.Update`/`base.Draw`
and exactly once per frame (fails without the change); skipping `base.Update` still updates it.
Integration 217/217.

### 2026-10-01 -- CSX-089: a Model tagged with the game's own type

Found with cna-cs-samples HeightmapCollision (CSSAMPLE-049): `terrain.xnb` is a `Model` whose `Tag`
is the game's `HeightMapInfo`, written with the game's `HeightmapCollision.HeightMapInfoReader`. A
root `Model` is read by CNA.Framework's XNB parser, which knows XNA's readers only, and it refused
the file. The facade's managed `ContentReader` resolves a game's readers as XNA does and already
has a `ModelContentReader` (for models nested in managed content). The parser now refuses an
unknown reader with `XnbUnsupportedReaderException` -- before anything is created -- and the
facade's root-`Model` path reads exactly that case through the managed reader. A check of the type
table up front was tried first and was wrong: the parser reads `VertexDeclarationReader` structurally
inside the vertex buffer, so a table check sent ordinary models down the managed path (two
integration tests failed on it). `CompatModelTagTests`: a one-bone model whose tag uses a reader the
test defines loads with that tag; fails without the change. HeightmapCollision is 0 px from its C++
port.

### 2026-10-01 -- CSX-088: components initialize inside base.Initialize(), once

Found with cna-cs-samples SoundAndMusic (CSSAMPLE-060): its `Initialize` calls `base.Initialize()`
and then reads `button.TextureCenter`, a texture its `Button : DrawableGameComponent` loads in
`LoadContent` -- a `NullReferenceException` here. XNA's `Game.Initialize` initializes every component
not yet initialized, front of the list first, then calls `LoadContent` (XNA IL); its
`DrawableGameComponent.Initialize` loads the component's content on the first call, inside it, when
a device exists (IL). The facade's `base.Initialize()` only loaded the game's content, and its
`DrawableGameComponent.Initialize` loaded nothing: components were initialized by CNA's own
`Game::Initialize`, which runs after the game's override returns, and their content arrived after
their own `Initialize` bodies.

`Game.Initialize` now initializes each component in collection order (one a component adds while
initializing included) and then loads content; a per-game set makes the later native pass, through
the adapters, a no-op, and removal clears a component's mark so re-adding initializes it again.
`DrawableGameComponent.Initialize` loads content on its first call when an `IGraphicsDeviceService`
has a device, and skips the one native `LoadContent` that follows (device-recreation reloads still
pass). `CompatComponentInitializationTests`: a constructor-added component is initialized with its
content inside `base.Initialize()`, a component added while running likewise, each exactly once;
fails without the change. Integration 215/215, `CNA.XnaCompat.Tests` 272/272.

### 2026-10-01 -- CSX-087: content is found from any working directory

Found on Android, where the working directory is `/`: ColorReplacement's `Model` was "not found"
although its texture loaded. The managed loaders -- the `Model` probe, `XnbContainer`, `.cnj`
models and sidecars, media references -- combined `RootDirectory` with the asset name and asked
`System.IO`, which resolves a relative path against the working directory. XNA's
`ContentManager.OpenStream` opens a relative content path through `TitleContainer.OpenStream`,
i.e. under `TitleLocation.Path` (XNA IL), so only the game's directory matters. Reproduced on the
desktop: SimpleAnimation started from another directory died with "Content file 'tank' was not
found".

`XnaContentPath.ToFilePath` now probes (and re-cases) under the title directory while keeping the
caller's relative form, which `TitleContainer.OpenStream` needs; `ToTitleFilePath` gives the path to
open, used by every loader that touches the filesystem itself. `AudioEngine` is unchanged: XNA's own
constructor calls `Path.GetFullPath(settingsFile)`, against the working directory.
`XnaContentPathTitleTests` (non-parallel; moves the working directory away from the title): a
relative root is probed and re-cased under the title, a title file path opens from anywhere, an
absolute root is not moved; the first two fail without the change. `CNA.Framework.Tests` 638/638.

Still open: CNA's native `ContentManager` resolves a relative root against the working directory
too, so from another directory SimpleAnimation now gets past its `Model` and stops at the native
texture load. The content module cannot see `TitleLocation` (runtime sits above it), so the fix
needs a layering decision in CNA.

### 2026-10-01 -- CSX-070: an unchanged XNA sample on Android

Toolchain: Android SDK `~/Android/Sdk` (NDK 29.0.14206865, emulator 36.5, AVD `Medium_Phone`,
x86_64 Google APIs image), .NET 11 RC1 with the `android` workload (`~/deps/dotnet11`).

`scripts/Build-AndroidNative.sh` builds CNA's C API with the NDK into `../cna/cmake-build-android-<abi>`
(OPENGLES3, SDL3, compiled effects, shared C API) and stages, under `build-consumer/cna-native-android`,
the stripped `libcna_c_api.so` (261 MB with the NDK's default `-g`, 50 MB stripped, 3,208 `cna_*`
exports), the SDL libraries it needs, `libmain.so` and SDL's Java sources. CNA needed CBIND-136 first:
no libcurl, nlohmann_json or libopus in the NDK.

How a game runs (`eng/android`): SDL's own `SDLActivity`, unchanged (`com.libcna.cna.CnaGameActivity`
only gives it a name in the app's namespace; SDL's Java is compiled in, not bound), loads `libmain.so`
and runs `SDL_main` on its thread, where the window, GL context and event pump belong. Android creates
the app's `CnaGameApplication` -- and so .NET -- before any activity; it hands `libmain` an
`UnmanagedCallersOnly` entry, and SDL's thread calls the game's own `Main`, which blocks in `Game.Run`
as on a desktop. Two managed changes: the resolver loads `libcna_c_api.so` by bare name on Android
(the APK's library directory is the loader's, not `AppContext.BaseDirectory`), and `Game.Run` skips
`PosixSignalRegistration`, which throws `PlatformNotSupportedException` on Android and iOS.

cna-cs-samples `scripts/android-sample.sh <Sample>` generates a `net11.0-android` app around a sample's
unchanged sources and Content (APK assets), installs it on a headless read-only emulator
(`-no-window -no-audio -gpu swiftshader_indirect`) and captures the screen.

AimingSample (Release, 33 MB APK): content loads, draws at its 853x480 back buffer scaled 2.25x and
pillarboxed into 2400x1080 -- 0.3% of pixels differ from the desktop capture after scaling back, all
resampling edges; a long press moves the cat to the touched point (touch -> mouse through the scale);
Home then relaunch resumes with its textures (SDL keeps the EGL context); a relaunch after exit starts a
fresh process (SDL ends the old one). A Back tap did not exit: the press is down and up between two
updates -- fixed in CNA as CBIND-137, which also gives `GamePad(PlayerIndex.One).Buttons.Back`, the
only exit most phone samples have; after it AimingSample and TouchThumbsticks end on one Back tap.
Not covered: audio (the emulator runs with `-no-audio`), arm64-v8a (the script builds it, nothing
ran), a device that loses its GL context.

### 2026-10-01 -- CSX-063: every checked-in sample in a browser

cna-cs-samples `scripts/browser-requalify.sh` runs each row through `browser-sample.sh` and measures
its canvas against the row's desktop C# capture. All 30 build and run in headless Chromium without
a page error; 12 are pixel-identical to the desktop (AimingSample, ContentManifestExtensions,
GesturesSample, InputReporter, InputSequence, LocalizationSample, Orientation, PathDrawing,
Pathfinding, SafeArea, TransformedCollisionTest, WaypointSample) and the rest differ where they
animate or seed randomness. The table is in cna-cs-samples `NEXT.md`.

What the first pass found: `ContentManifestExtensions` failed with `Arg_NoDefCTor` on
`ListReader<string>` -- publishing trims, and the content manager builds readers by reflection
from the names an XNB declares, so `CNA.Browser.targets` now roots the CNA.NET assemblies and the
sample glue roots the game's and its libraries'. Pathfinding and SpriteSheet keep types in library
projects, which the glue now references as their own builds (content names readers by assembly).
ShapeRendering drew nothing in the browser and, it turned out, in every Release capture since the
migration: its `DebugShapeRenderer` is `[Conditional("DEBUG")]`, and the row is qualified in Debug;
the capture scripts now honour a row's `CnaSampleConfiguration`.

### 2026-10-01 -- CSX-062: an unchanged XNA sample in Chromium

Tooling, all checked in: `scripts/Build-BrowserNative.sh` builds CNA's WebGL2 C API with the .NET 11
workload's own Emscripten and stages the merged archive (CNA CBIND-135) as
`build-consumer/cna-native-browser-wasm/cna-native.a` with a provenance file;
`eng/browser/CNA.Browser.targets` makes a `Microsoft.NET.Sdk.WebAssembly` project link it (native
relink, JS exceptions, 64 MB initial heap, WebGL2); `eng/browser/wwwroot` is the default page
(`#canvas`, `runMain()`); `scripts/Run-BrowserPage.mjs` serves a bundle to headless Chromium, drives
`CNA_ACTIONS` (mouse, keys, reload) and captures the canvas. cna-cs-samples'
`scripts/browser-sample.sh` generates a browser project from a sample's evaluated properties and
Compile items, so the sample's own files stay as they are.

AimingSample, unchanged: its XNBs load from the browser's file system; the capture is 853x480, the
back buffer the sample asks for, and **0 pixels** differ from its C++ port's desktop capture (a
2,666-pixel difference was Chromium's focus ring on the canvas perimeter -- the default page now
turns it off). Held ArrowRight moves the cat right; a held left button pulls it toward the pointer;
holding Escape takes the sample's own exit, and `CNA: the game's run has ended.` (the loop now says so,
as an exited game otherwise just freezes its canvas); a reload starts it again. A key pressed and
released inside one animation frame is not seen, as with any XNA game polling its keyboard once
per update.

Not run: Chrome on a desktop with a GPU (SwiftShader here); audio in the browser; a game with
compiled effects.

### 2026-10-01 -- CSX-061: Game.Run in a browser

An XNA game's `Main` is `using (var game = new Game1()) game.Run();`, and a page cannot block in it.
Native gained the host-driven run (CNA CBIND-134, ABI 0.38.0, admitted in `ba65cd2`); the CNA layer's
`Run` now, in a browser, starts `BrowserGameLoop` and returns. The loop asks for animation frames
itself -- a `[JSImport]` of `globalThis.requestAnimationFrame` with a managed callback -- so a page
needs only `runMain()`; each frame calls `cna_game_run_frame_ext`, which begins the run on the first
and ends it (`Exiting`, `EndRun`) on the one that finds the game exited. The `Dispose` the `using`
performs as soon as `Run` returns waits, in both layers, until the run has ended. A frame whose
callback threw ends the run before the exception reaches the page. Only one game runs per page.

Measured in headless Chromium (`build-consumer/browser-probe`, the binding unchanged but for this):
ABI 0x2600 admitted from the linked library; `Main returned` straight after `Run`; `LoadContent
800x480`, `BeginRun`, 180 updates at 60 Hz until the game's own `Exit()`, `Exiting`, `EndRun`,
`Disposed`. Desktop unchanged: framework 635, XnaCompat 272, integration 213, GamerServices 24.

Not yet: an `Exit` in the browser leaves the last frame on the canvas (there is no window to close);
packaging -- a host template, the CNA archives and the MSBuild glue a game project imports; a real
sample with content, input and audio in Chrome (CSX-062).

### 2026-10-01 -- CSX-086: the game lifecycle arrives in XNA's order, Disposed included

Found by the browser probe's log, true on the desktop as well. The facade's `Run` called `BeginRun`
itself before handing over to native, so it ran before `Initialize` and `LoadContent`; XNA's
`RunGame` calls it after them, before the first `Update`. `EndRun` sat in a `finally`, so it also ran
when a callback threw, which XNA's does not. And no game's `Disposed` ever fired: native raises it
inside `cna_game_destroy`, after the game has released the event bridges that would deliver it.

Fix: the CNA layer hooks native's `begin_run`/`end_run` (`OnBeginRun`/`OnEndRun`, new `BeginRun` and
`EndRun` virtuals) and the facade's backend forwards them, so they arrive where native's own run puts
them -- for `Run` and for any other run alike. `Dispose(true)` raises `Disposed` itself, last, as
XNA's does.

Tests: `CompatGame_RunDeliversXnasLifecycleOrder` (`Initialize LoadContent BeginRun Update Exiting
EndRun Disposed`; with the old shape `BeginRun Initialize ...`), `CompatGame_DisposeRaisesDisposedOnce`
(0 before). Integration 213/213.

### 2026-10-01 -- CSX-060: a C# XNA game draws in a browser

A browser app links its native code into `dotnet.native.wasm` with the Emscripten its `wasm-tools`
workload pins, so the toolchain is .NET's, not CNA's. .NET 8 pins Emscripten 3.1.34 and .NET 10
3.1.56; both ship libc++ 17, which has no `std::jthread` (CNA's Net and GamerServices workers).
.NET 11 (RC1, go-live; GA next month) pins 6.0.3 with libc++ 21 and compiles CNA unmodified, so the
browser host targets `net11.0` while the binding stays `net8.0`. Toolchain: user-local SDK in
`~/deps/dotnet11` plus `dotnet workload install wasm-tools`.

CNA: `cmake-build-webgl2` configured with the workload's `emcmake` (WEBGL2, SDL3, compiled
effects; its own `CNA_SDL_PREBUILT_ROOT`). Building it found CNA CBIND-133 (`e77ae09ed`):
`GraphicsResource.hpp` held a `unique_ptr` to a forward-declared lease, which libc++ rejects.
`generate_static_archive.py` partial-links with the host `ld`, which cannot read wasm objects, so
the probe links the 39 archives of `cna_c_api_wasm`'s closure directly (wasm-ld is order-blind).

The probe (`build-consumer/browser-probe`, gitignored): `Microsoft.NET.Sdk.WebAssembly`,
`WasmBuildNative`, `WasmEnableExceptionHandling=false` (CNA's Emscripten exception ABI is the
JS-lowered `-fexceptions`), `EmccInitialHeapSize` 64 MB (CNA's static data is 37 MB),
`NativeFileReference` `cna-native.a` (the name the `DllImport` resolves to) plus the closure, a
`[JSExport]` frame pumped by `requestAnimationFrame`. Three binding defects stood between it and a
frame: the resolver threw for want of a library file (it now defers on the browser, and the module
initializer admits the linked library's ABI by the same policy); handles narrowed to 32 bits
(CSX-085); and two imports took a `delegate* unmanaged` parameter, for which Mono's wasm
interpreter has no trampoline -- `GraphicsDeviceManager`'s constructor aborted the runtime. Those
now take `nint`, abi-verify pairs their C callback types, and `Imports_TakeNoFunctionPointerParameter`
fails if one returns (checked by reintroducing one).

Result in headless Chromium 1234 (SwiftShader WebGL2): `EasyGLRenderer initialized with ... WebGL
2.0`, ABI 0x2500 admitted, `LoadContent` at 800x480, 121 frames at 60 Hz, CornflowerBlue clear and a
SpriteBatch quad where the game puts it. Chromium only gets WebGL with `DISPLAY` unset.

Not yet: `Game.Run` in a browser (CSX-061; frame stepping skips `BeginRun`/`EndRun`/`Exiting`, so
native needs a host-driven run route); packaging the archive and host glue; a real sample, input,
audio, content and reload in Chrome (CSX-062).

### 2026-10-01 -- CSX-085: handles are 64-bit everywhere

The first C# game in a browser (CSX-060) died creating its content manager:
`OverflowException` in `CnaHandle.AsNint`. A `CNA_Handle` is `uint64_t` on every platform, and the
binding narrowed it into `nint` -- `NativeResourceHandle` was a `SafeHandle`, which stores a
pointer-width value -- so on wasm32 (and any 32-bit Android ABI) every real handle overflowed.

Fix: `NativeResourceHandle` is a `CriticalFinalizerObject` over the `ulong`, keeping what
`SafeHandle` gave it -- release at most once from `Dispose` or the finalizer, none for a borrowed
(`ownsHandle: false`) or detached handle, `IsClosed`/`IsInvalid`, the owner-thread release queue.
`CnaHandle` lost its `nint` constructor and `AsNint`; the compiler then named every narrowing site
(about 250 across 90 files: `NativeHandleValue` properties, wrap constructors, release callbacks,
handle-returning helpers). OS window handles stay `nint`, as XNA's `IntPtr`.

Tests: `Handle_HoldsTheFull64BitValue` (a value above 2^32 round-trips to the release callback;
the type is not a `SafeHandle` and hands out `ulong`), `Handle_ReleasesAtMostOnceAndNeverABorrowedOrDetachedOne`.
Framework 633/633, XnaCompat 272/272, integration 211/211, GamerServices 24/24 on the desktop; the
browser probe gets past the content manager.

### 2026-10-01 -- CSX-050: the checked-in samples, requalified

cna-cs-samples `2d62243..bd9d069`. `scripts/requalify.sh` builds each of the 30 rows in Debug and
Release, captures it on a private Xvfb, takes its exit path and captures the row's C++ port the
same way. All 30 build, run and capture against cna-cs `496858b` and CNA `e9dd5d879`; every row with
an Escape path exits 0; 11 are pixel-identical to their C++ port, three more (GesturesSample,
Orientation, PathDrawing) once the retained C++ phone captures' 100-pixel offset is removed, and the
rest differ by animation phase or seeded randomness. Found on the
way: CSX-083 (ColorReplacement's tyres) and CSX-084 (Pathfinding's SIGTERM hang). Phone-only rows
run through a host generated from `<CnaPhoneGame>`, with CNA.PhoneCompat; InputSequence compiles now
that the Net namespace exists. 28 of 78 eligible rows are done; the table is in cna-cs-samples
`NEXT.md`.

### 2026-10-01 -- CSX-084: a terminated game exits through its own loop

Requalifying the samples hung on Pathfinding, which has no exit key, so the capture script ends it
with SIGTERM and then stops Xvfb. Native stacks: the runtime's SIGTERM default had called `exit()`
on its signal thread, and libGLX's destructor waited there for the Xlib display lock; the game
thread held that lock in `XSync`, hit the dead X server, and its IO-error `exit()` waited for the
first. The same race is the crash `docs/native-behavior-blockers.md` recorded for the template.
A C++ CNA game never meets it: SDL turns SIGINT/SIGTERM into a quit event, but installs its
handlers only where none exists, and .NET always has one.

Fix: `CNA.Game.Run` registers SIGTERM and SIGINT for its duration; the handler sets a flag and
cancels the default, and the next update callback asks native to exit, so `Run` returns, `Exiting`
is raised and the process ends through `Main`. A second signal before that falls through to the
default, so a game that stopped updating can still be killed.

Test: `Sigterm_EndsRunThroughTheGamesOwnExit` sends SIGTERM to its own process at frame 3 and
asserts `Run` returned through `Exiting` well before the test's own give-up frame; with the fix
reverted the test host crashes ("Test host process crashed"). Results: integration 211/211,
framework 631/631, XnaCompat 272/272, GamerServices 24/24.

### 2026-10-01 -- CSX-083: the device's state cache follows native SpriteBatch

ColorReplacement (CSX-050) drew the car without tyres or headlights. A probe drawing the same model
to the back buffer drew them; adding one `SpriteBatch` string made them vanish. The facade's
`BlendState`/`DepthStencilState`/`RasterizerState`/`SamplerStates[i]` setters skip an assignment of
the object they last set, as XNA's do -- but XNA's SpriteBatch assigns its states through those
same properties (`SetRenderState`), while here native End applies them itself. The cache kept the
game's `Opaque`, native held the batch's `AlphaBlend`, and the sample's next `BlendState = Opaque`
was skipped; the tyre texels carry alpha 0 (Car_0's alpha is the colour-replacement mask) and blended
away. The C++ port has no such cache and drew them.

Fix: the compat SpriteBatch tells the device the states native applied (End for deferred modes,
an empty batch included; Begin for Immediate; null meaning the slot default), keeping XNA's object
identity (`Assert.Same(BlendState.AlphaBlend, device.BlendState)` after End). The CNA layer has no
early-out but cached its getters, so its SpriteBatch drops them for the next read to query native.
`BlendFactor`/`MultiSampleMask` and `ReferenceStencil` now dirty the blend and depth-stencil state as
XNA's setters do, so re-assigning the same object re-applies it. The early-out stays: re-assigning
the active, disposed state object must remain a no-op (XNA IL; native mirrors it).

Tests: `CompatSpriteBatch_StateAssignedAfterABatchReachesTheRenderer` (transparent black quad under
`Opaque` after a batch reads back black; CornflowerBlue before the fix),
`CompatSpriteBatch_AppliedStatesAreTheDevicesStates`,
`CompatGraphicsDevice_ReassigningAnOverriddenStateReappliesIt`,
`SpriteBatch_EndLeavesTheBatchStatesOnTheDevice` (CNA layer); each fails with its mechanism removed.
Results: integration 210/210, framework 631/631, XnaCompat 272/272. ColorReplacement re-captured: tyres
present, 2.24% of pixels differ from the C++ capture (rotation phase at the edges) against 5.25% before.

### 2026-10-01 -- CSX-080: XNA's exception types, from the exception native actually threw

Result and category cannot name the exception an XNA call throws: `GamerPrivilegeException`,
`GuideAlreadyVisibleException` and `InvalidOperationException` are all `INVALID_STATE`;
`ArgumentOutOfRangeException` is `INVALID_ARGUMENT` like its base. Samples catch exactly these:
`NetworkException` 10, `GamerPrivilegeException` 10, `NoMicrophoneConnectedException` 6,
`NoAudioHardwareException` 4, `ContentLoadException` 2, `InstancePlayLimitException` 1 (grep over
the upstream samples).

CNA `e9dd5d879` (CBIND-132, ABI 0.37.0): the exception barrier notes the dynamic type of the
exception it translates -- demangled, `::` read as `.`, which is the .NET name because CNA's C++
namespaces are the .NET ones -- and an argument exception's `ParamName`; four count/copy routes
read them back. cna-cs admits 0.37.0 (retires 0.36.0; package acceptance refused a stale 0.36.0
library left in its native directory, by name), `CnaException` gains `CanonicalExceptionType` and
`CanonicalParamName`, CNA.Interop builds the `System` ones, and CNA.XnaCompat's `XnaExceptions`
maps the XNA ones. The CNA layer keeps throwing `CnaException` -- its contract -- and the facade
re-raises where it owns the call: GamerServices/Guide/Avatar/Net (`GamerServicesInterop.Check`),
CNA.PhoneCompat (sensor failures with their error id), `AudioEngine`/`WaveBank`/`SoundBank`,
`SoundEffect.CreateInstance`/`Play`, `SoundEffectInstance.Play`, `Microphone.Start`,
`MediaPlayer.Play`. A global hook was rejected: it would change the CNA layer's exceptions for any
process that also loads the facade.

Tests: `Guide_RefusesWithXnasOwnArgumentExceptions` (`ArgumentOutOfRangeException` `focusButton`,
`ArgumentException` `title` -- before, a bare `ArgumentException` without a parameter);
`ResourceIntegrationTests` asserts a misaligned dynamic buffer's `System.ArgumentException`/`buffer`
through the CNA layer. Results: framework 631/631, XnaCompat 272/272, integration 206/206,
GamerServices 24/24, abi-verify 1409 prototypes 0 mismatches, all api-compat profiles 0, package
acceptance passed. Not yet: the other facade calls (graphics, input, storage...) still surface
`CnaException`; the audio boundaries are not provoked by a test (no way to exhaust instances or
lose audio hardware here).

### 2026-10-01 -- CSX-081: the default GraphicsProfile comes from the project's XnaProfile

XNA's `GraphicsDeviceManager` constructor reads the game assembly's
`Microsoft.Xna.Framework.RuntimeProfile` resource (first line ends in `Reach`/`HiDef`; Reach when
absent), which XNA's build wrote from `<XnaProfile>` -- 93 of the upstream sample projects say
HiDef. The facade read nothing and every C# game ran Reach. Now the facade's constructor reads the
resource exactly as the IL does, and `src/CNA.XnaCompat/build/CNA.XnaCompat.targets` writes it
(`<XnaPlatform>.v4.0.<XnaProfile>`, an unknown profile is a build error), packed as `build/` and
`buildTransitive/`.

Found on the way: `a07f385`'s fixture symbol list missed the last phone import (`0c18dca`), caught
by package acceptance's ABI gate.

Tests: `RuntimeProfileTests` (XnaCompat.Tests itself declares `XnaProfile=HiDef` and imports the
targets): the resource is `Windows.v4.0.HiDef`, the reader answers HiDef for it and Reach without
it. Package acceptance builds the isolated consumer with `-p:XnaProfile=HiDef` and checks the
resource (`PACKAGE_XNA_PROFILE_RESOURCE=passed`). XnaCompat 272/272, integration 206/206,
GamerServices 23/23. Not done here: cna-cs-samples importing the targets and each sample declaring
its original profile -- with CSX-050, since it changes what those rows render.

### 2026-10-01 -- CSX-045: CNA.PhoneCompat, the opt-in Windows Phone device assembly

Seven gallery samples reach Windows Phone SDK types (`DEC-001` in cna-cs-samples); the owner brief
settled it as a separate opt-in assembly. `src/CNA.PhoneCompat` provides what they use:
`Microsoft.Devices` (`DeviceType`, `Environment.DeviceType`, `VibrateController.Default`) and
`Microsoft.Devices.Sensors` (`Accelerometer` with `IsSupported`, `State`, `Start`/`Stop`,
`CurrentValue`, `IsDataValid`, `TimeBetweenUpdates`, `CurrentValueChanged` and the 7.0
`ReadingChanged`; `SensorBase<T>`, `AccelerometerReading`, `ISensorReading`,
`SensorReadingEventArgs<T>`, `AccelerometerReadingEventArgs`, `SensorState`,
`SensorFailedException`/`AccelerometerFailedException`), over 21 devices.h/sensors.h routes.
`System.IO.IsolatedStorage` needs nothing: .NET 8 provides `GetUserStoreForApplication` on Linux
(measured).

No Windows Phone reference assemblies exist here, so the metadata is unmeasured; the authority is
CNA's C++ phone headers (from archived MSDN pages) and the samples. Behaviour follows native: a
desktop is `DeviceType.Emulator` (only a mobile platform answers `Device`), and a desktop
accelerometer is unsupported, so `Start` throws `AccelerometerFailedException` with native's error
id -- the exception those samples catch to fall back to keys. The C++ gallery ports do not enable
CNA's keyboard accelerometer emulation, so for parity neither does this.

Evidence: unchanged sources of AccelerometerSample, Bounce, CameraShake, MarbleMaze, Platformer and
SoundAndMusic compile with `WINDOWS_PHONE` against XnaCompat + PhoneCompat (probes in
`build-consumer/phonecompat-probe`, not committed); Yacht also needs its WCF `YachtServices`
sibling and `Microsoft.Phone.Shell`/`Notification` (CNA C++ has `modules/phone`; no C routes yet).
`PhoneCompatTests` (3): environment and vibration with XNA's duration check; an unsupported
accelerometer's state and refusal; readings through both events in native's order, in g, via
native's test routes. abi-verify: 1405 prototypes, 23 callbacks (the two reading callbacks), 604
constants (`DeviceType`, `SensorState`), 1137 layout values, 0 mismatches. Integration 206/206.
Packaging of `CNA.PhoneCompat` as a NuGet package is not done.

### 2026-10-01 -- CSX-043: Microsoft.Xna.Framework.Net over CNA's network sessions

All 17 Net types against the XNA IL: `NetworkSession` (every Create/Find/Join/JoinInvited overload
and its Begin/End pair, the session state machine, rosters, properties, simulated latency/loss,
the nine instance events and the static `InviteAccepted`), `NetworkGamer`, `LocalNetworkGamer`
(every SendData/ReceiveData overload), `NetworkMachine`, `AvailableNetworkSession(Collection)`,
`NetworkSessionProperties`, `QualityOfService`, `PacketReader`/`PacketWriter` (managed
`BinaryReader`/`BinaryWriter`, raw IEEE bits as XNA writes them) and the event args. XNA's own
argument checks run first with its parameter names; join failures carry their
`NetworkSessionJoinError`; `GamerJoined` replays existing gamers to a new handler; a handler's
exception is rethrown from `Update`; one managed object per gamer and per machine.

Found on the way and fixed: (1) `b0fbab8` -- owned signed-in gamer handles were released through
the plain gamer route, which refuses them, so every `Gamer.SignedInGamers` lookup leaked a handle;
(2) the session callbacks first went in with two parameters where CNA passes three
(session, info, context), crashing on the first event. abi-verify had not caught it because the
callbacks cross as `nint`; its callback check now covers every GamerServices and Net callback
XnaCompat passes (6 -> 21), with `const` on a lent event description the only accepted
difference.

Tests (`NetTests`, 10): identity across rosters/host/lookup/machine, the replay, Lobby -> Playing ->
Lobby with events from `Update`, a handler exception out of `Update`, a SystemLink packet looped
back through native ENet into a managed `PacketReader` byte-exactly (22/22 bytes, via 0.36.0's
copy route), the Local session's drop, properties (session-bound and standalone), XNA's argument
checks, Dispose then a second session, Begin/End.

Results: GS/Net profile 75/75 with 0 diagnostics, runtime 256/256, leak-only 0; GamerServices
integration 23/23; integration 203/203; framework 630/630; XnaCompat 269/269; abi-verify 1384
prototypes, 21 callbacks, 0 mismatches. Not measured: two machines (SystemLink between two
processes, host migration, `RemoveFromSession`), PlayerMatch/Ranked against a CNA service, voice.

### 2026-10-01 -- CSX-044: ABI 0.36.0 for the managed PacketReader; CBIND-131 found on the way

XNA's `PacketReader` is a `BinaryReader`, so `LocalNetworkGamer.ReceiveData(PacketReader, ...)`
needs the received bytes in managed memory. The C ABI could size a native reader to the next packet
but not read it back, and the byte-array route refuses a buffer smaller than a packet whose size no
route reports. CNA `402c1aaa9` (CBIND-130) adds `cna_packet_reader_copy_data_ext`, ABI 0.35.0 ->
0.36.0, one export added and nothing else (`NetSmoke.c` copies a received packet byte-exactly).

Verifying it, `CApi_TeardownLifetime_cycles` segfaulted intermittently at exit: EasyGL's context
lease state was a `static thread_local` that `exit()` had destroyed before the handle registry's
fallback disposed a live game. CNA `abd005ab8` (CBIND-131) keeps it on the lease control; 40/40
looped runs clean (crashed at run 13 of 15 before).

cna-cs admits 0.36.0 and retires 0.35.0: `baselinediff.py` 6e0de68e8 -> 402c1aaa9 has 0 breaking
differences (allowlist now empty), abi-verify 1119/1119 values and 1384/1384 prototypes, fixtures 2
accepted / 10 rejected (`retired-0.35.0` refused on the version rule alone), framework 630/630,
integration 203/203, GamerServices 12/12, package acceptance passed with the reinstalled 0.36.0
`CNACApi` component. CI pins `402c1aaa9`, previous accepted `6e0de68e8`.

Of the two other gaps, Windows XNA's own `LeaderboardWriter.GetLeaderboard` and
`Gamer.LeaderboardWriter` throw `NotSupportedException` (a Pro feature there), so the facade already
matches the reference profile; CNA's C++ does write leaderboards and a session route would go beyond
it.

### 2026-10-01 -- CSX-042: Avatar description, animation and renderer

`AvatarDescription`, `AvatarAnimation` and `AvatarRenderer` against the XNA IL, over CNA's native
avatar routes. XNA semantics kept: a signed-in player's description is one object until that
avatar changes (slot per player, emptied by the native shared `Changed` event, which is then raised
with the gamer as sender); `EndGetFromGamer` may be called repeatedly; the description bytes are
copied out; animation position/length/face/bones are the object's own state, refreshed by `Update`,
one `BoneTransforms` collection changing in place and readable after `Dispose`; renderer
transforms and lights are its own fields (initialized from native's defaults, handed to native at
`Draw`), `BindPose` refuses until `Ready`, `State`/`Draw` refuse after `Dispose`.

Tests (`AvatarTests`, 5): byte round trip and XNA's argument checks; per-player identity through
Begin/End; animation state after dispose; renderer draws inside a frame; a renderer goes
Loading -> Ready (116 ms on build-probe) and then exposes a 71-bone bind pose. Not exercised: a
stored avatar changing under a signed-in player (no test route to change it).

Results: GS/Net profile 58/75 (17 Net types missing), GamerServices integration 12/12.

### 2026-10-01 -- CSX-040/041: GamerServices and Guide over CNA's native gamer services

cna-cs over CNA `6e0de68e8`. CSX-040 (`fbff5e7`) bound every GamerServices, Guide, Avatar and Net
route of the 0.35.0 headers: 426 imports, 98 prototype overrides each derived from the compiler's
diagnostic and limited to the four representational differences abi-verify documents. CSX-041
puts XNA's `Microsoft.Xna.Framework.GamerServices` on them, member by member against the XNA IL:
`GamerServicesDispatcher`/`GamerServicesComponent` (process-wide, initialize-once, IL order),
`Gamer`/`SignedInGamer`/`FriendGamer` (one managed object per native gamer, kept through the native
gamer tag), collections, presence, profiles, achievements, leaderboards (reader, entry, identity,
`PropertyDictionary`), `Guide` (message box and keyboard genuinely asynchronous, completing on the
game thread; `GuideAlreadyVisibleException` checked first), the event args and exceptions, and the
enums (values asserted against the headers by abi-verify, now 596 constants). Native refusals map
to XNA's exception types for these calls (`ArgumentException`, `InvalidOperationException`,
`NotSupportedException`, `ObjectDisposedException`); a game callback's exception is rethrown from
`Game.Run` (via CSX-082's path) instead of unwinding through native.

Two C ABI gaps refuse explicitly: `LeaderboardWriter.GetLeaderboard` (no session-writer route) and
a non-empty `PropertyDictionary` stream's contents (only presence and length cross) -- CSX-044.

Tooling: api-compat profiles gained `excludedNamespacePrefixes`; the runtime profile excludes
GamerServices/Net (256 types: `GamerServicesComponent` lives in `Game.dll` and moved to the GS/Net
profile). `CompatEnumParityTests` exempts the two namespaces, whose values abi-verify checks
against the C headers instead.

New `tests/CNA.GamerServices.IntegrationTests` (own process; the dispatcher is process-wide):
`gamerservices.runsettings` keeps native gamer services off the owner's keyring, profiles and XDG
directories and auto-signs in `CnaTester` (`Environment.SetEnvironmentVariable` does not reach
native `getenv` on Unix, so it must be the process environment).

Results: build 0/0; Framework 629/629; XnaCompat 269/269; integration 203/203 and GamerServices
7/7 on `run_gpu_tests_private.sh`; `Verify-Abi.sh` 1119/1119 values, 1383/1383 prototypes, 0
mismatches; runtime profile 256/256 with 0 diagnostics; GS/Net profile 55/75 (20 missing:
`AvatarAnimation`, `AvatarDescription`, `AvatarRenderer`, 17 Net types); leak-only 0.

Known risk, not yet fixed: native keeps the dispatcher initialized after its game is destroyed, so
a second game in the same process inherits it.

### 2026-10-01 -- CSX-030: template and package consumers

`cna-cs-template` (`ae6b906`) against the migrated binding: the checked-in game draws 600 frames on
OPENGLES3; `scripts/verify-template.sh --mode development` (fresh `dotnet new` consumer) builds and
draws 60 frames. Its generated consumers had been built in `/tmp` and run on any `DISPLAY`; they now
go to cna-cs `build-consumer/template-<mode>` (inside the template, `dotnet new` copied them into the
next game: 24 duplicate-member errors) and run on CNA's private display runner.

Packaging found a hidden absolute path: the native asset was the build tree's `libcna_c_api.so`,
whose RUNPATH names `cna/.sdl-prebuilt-.../install/lib`, so a package consumer loaded SDL from this
machine's CNA checkout. `CNA.Interop` now packs an installed `CNACApi` component directory
(RUNPATH `$ORIGIN` + `libSDL3*.so.0`) and `Package-Acceptance.sh` refuses any other RUNPATH.
`cmake --install ../cna/build-probe --component CNACApi --prefix build-consumer/cna-native-linux-x64`
then `scripts/Package-Acceptance.sh --native-directory build-consumer/cna-native-linux-x64/lib`:
passed (3 packages, template dev + package, isolated consumer 60/600 frames with no native env,
missing/wrong-arch/wrong-ABI/missing-symbol/invalid-path/conflict/override diagnostics). `ldd` on
the consumer's packaged library resolves SDL from its own `runtimes/linux-x64/native`.

Not done here (CSX-031): a platform layout for browser/Android, which waits for those platforms.

### 2026-10-01 -- CSX-021/022/052, CBIND-129, CSX-082: historical blockers re-run

Re-ran the four `⛔` cna-cs-samples rows on `build-probe` with `scripts/capture-sample.sh` (private
Xvfb :128). SpriteEffects and WaypointSample ran and exited 0 at once. ParticleSample and
ColorReplacement first surfaced only a message (`Callback: Object reference not set...`), so
CSX-082 made callback exceptions unwind with their own type and stack; that exposed both causes:

* **CNA CBIND-129 (`6e0de68e8`)**: `ForwardingComponent::Initialize` ran the base (which loads a
  drawable's content) before the component's own initialize handler. ParticleSample sets its
  texture name in `Initialize`. Handler first now; `RuntimeComponentsSmoke.c` records the sequence
  and fails on the old order. `^CApi` 117/117.
* **cna-cs `5f6c212` (was CNA-REPORT-002)**: `ModelMesh.Draw` disposed the effect's cached
  technique/pass wrappers each draw; the second draw used a released handle. CNA was correct. New
  `CompatModel_DrawsRepeatedlyWithTheEffectsOwnTechnique` fails without the fix.

All four rows now run and exit 0 through Escape (`/rv/tmp/cs-samples/requal-20261001/`);
ColorReplacement renders with its compiled effect, which also closes CSX-022 on this tree.
REPORT-001's root (resizable XNA window) was already fixed in CNA `8c713f1d8`; REPORT-003 did not
reproduce. cna-cs-samples `26d06c1` records this and replaces its obsolete read-only/stop rules
(CSX-052). cna-cs `docs/native-behavior-blockers.md` re-measured against 0.35: two rows closed,
seven re-confirmed open from the headers, five explicitly marked not re-measured.

Integration 203/203 (Release), framework 629/629, XNA-compat 225/225.

### 2026-10-01 -- CSX-010..014, CSX-020: ABI 0.35 migration and component device borrow

**CNA `bdf239360` (CBIND-128).** Reproduced CNA-REPORT-004 natively with the new pure-C test
`RuntimeComponentDeviceBorrow.c`: `load 0/1, update 0/4, draw 0/4 borrowed; re-entry 0`. Root cause:
`CGame` scoped the device borrow to the consumer's own callback; `Game::Update/Draw/Initialize` drive
components after it returns, and component callbacks could even re-enter `run_one_frame`. Fix: a
depth-counted callback scope over each lifecycle step plus each C component handler. `^CApi` 117/117
(`tools/platform/run_gpu_tests_private.sh build-probe -R '^CApi' -j8`, `SDL_AUDIODRIVER=dummy`), route
coverage 3202/3202, declared==exported.

**cna-cs.** Removed 44 retired engine-layer imports, `cna_graphics_ext_is_available`, the
`CNA.Graphics.Experimental` layer, `CnaPostProcessContext`, the engine-layer helpers and their tests.
Header-doc diff of the 957 consumed routes found 14 changed contracts; acted on: `Clear(Color)` now
XNA's `Clear(DefaultClearOptions, color, 1f, 0)` (XNA IL), `ShaderDialect` +HLSL/MSL/WGSL/SPIRV.
Four integration tests encoded non-XNA behaviour and were corrected (quad behind the camera, Reach
separate alpha, Reach occlusion query, misaligned audio buffer -- XNA's own size arithmetic gives
88 198 bytes for 1 s at 44.1 kHz mono). New C# test
`DrawableComponent_UsesGraphicsDeviceFromGameDrivenCallbacks`: fails on the pre-fix 0.35.0 artifact
(`~/deps/cna-c-abi-0.35.0-opengles3-fx`, InvalidState), passes on `bdf239360` (viewport 800).

Commands and results (all on `cna/build-probe`, OPENGLES3 Release, compiled effects ON):

* `scripts/Verify-Abi.sh` -> 895/895 values, 957/957 prototypes, 0 mismatches, 12/12 controls
* `scripts/Verify-NativeAbiCompatibility.sh --native-library ../cna/build-probe/modules/c-api/libcna_c_api.so` -> 957 required, 2 accepted, 10 rejected, real library passed
* `python3 tools/coverage/baselinediff.py --from ../cna@599d14e54 --to ../cna@bdf239360 --allowlist eng/cna-upstream-abi-allowlist.txt` -> exit 0, 1048 reviewed, 0 stale (allowlist now supports `*` globs, each must still match)
* `tools/platform/run_gpu_tests_private.sh --exec <wrapper> <lib>` running `dotnet test tests/CNA.Integration.Tests -c {Release,Debug} --no-build` with `CNA_NATIVE_LIBRARY`, `SDL_AUDIODRIVER=dummy` -> 200/200 both
* `dotnet test tests/CNA.Framework.Tests` 629/629, `tests/CNA.XnaCompat.Tests` 225/225
* `XNA_REFERENCE_PATH=~/deps/xna40-windows-assemblies dotnet run --project tools/api-compat -c Release -- --format text` -> 257/257, 0 diagnostics

Open from this step:

* CI pins `libcna/cna@bdf239360`, which exists upstream only after the owner pushes CNA.
* Facade surfaces `CnaException` where XNA throws `InvalidOperationException`/`NotSupportedException`
  /`ArgumentException` for the same refusals (occlusion sequencing, Reach limits, misaligned audio
  buffers) -- CSX-080.
* Facade `GraphicsDeviceManager` ignores XNA's embedded `Microsoft.Xna.Framework.RuntimeProfile`
  resource, so every C# game runs Reach even when its project says HiDef -- CSX-081.

### 2026-10-01 -- session 1 start

HEADs at start: cna `next` 8b55f7f4d; sharp-runtime `feature/gamer-services-collections`
88f6b11f (`next` is e03e8465); cna-cs `develop` e223909; cna-cs-template `develop` 35e9700;
cna-cs-samples `develop` 69ddcc1; samples.libcna.com `main` 8e48825. All clean.

Toolchains: .NET SDK 8.0.424 (no workloads), emsdk 6.0.9 at `~/emsdk`, Android SDK at
`~/Android/Sdk` (ndk, emulator, system-images present), google-chrome.

CNA C ABI is 0.35.0 (`modules/c-api/include/CNA/C/abi.h`). `baselinediff.py --from
../cna@599d14e54 (0.21.0) --to ../cna (0.35.0)`: of 1002 consumed exports 44 absent, 0 changed
prototype; none of the changed constants (`CNA_CNB_SOUND_EFFECT_SCHEMA_VERSION` 1->2, four
`*_MAXIMUM` sentinels) or the changed `CNA_AvatarRendererInfo` layout is consumed by cna-cs.
