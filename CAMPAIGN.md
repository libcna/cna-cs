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
| CSX-104 | Real XNA 4.0 games beyond the gallery, unchanged (cna-cs-samples `games/`) | done on the Linux desktop: 65 games, the seven XNA 4.0 shader tutorials, the fourteen samples of *2D Graphics Programming for Games*, nineteen of *Windows Phone 7 Game Development*, three 3D course projects and four samples of Red Badger's XPF UI framework run -- Speedy Blupi, Rookie Drivers, TIE Fighter Forever, Resonance, Microsoft's Solitaire and Moto Trial Racer, NePlus, the XNASidescroller, Virulent, Kosmic Warz, Dominó Tropical, A Princess' Request, Swf2XNA's My Big Head and Playing in Traffic, Escape From Enceladus (2026-10-02, after CNA FX-141, CSX-109 and CSX-110), __Defense (after CNA CBIND-145, CSX-111 and CSX-112), Missile Command, a Super Mario World demo, a Zelda clone, Bubble Bound, Spineless (after CNA CBIND-146) Jomata's Mahjong (which found CNA CBIND-147/148) and the XNA 4.0 Racing Game Kit (menu and a race; CSX-113, CSX-114, CNA FX-142) play or reach their menus; HeliumBiker (Wii Remote), Zombie Smashers and Playing in Traffic (Xbox 360 gamepad) wait for the controller they read, as on Windows without one. Content their repositories ship, or built from their own content projects by XNA's BuildContent under Wine (songs and videos as labelled stand-ins); prebuilt XNA libraries run through CSX-099. Four XNA builds compared with XNA under Wine (title frames 8 px to 0.32%). Nineteen recorded with the reason outside XNA (Snails' published source builds on no platform; two need commercial fonts; one needs its defunct server) (Windows API, Windows Forms, Silverlight, files their repositories do not ship). Found CSX-105/106/107 and CNA Task 1120, CBIND-142. In a browser 14 of the 16 reach their title or play (NePlus and Princess Request after generator fixes; Resonance its menu, its levels load on a thread); HeliumBiker starts a thread (single-threaded WebAssembly) and Playing in Traffic plays a video (no video backend in the browser build). On Android 15 of the 16 run on the emulator (Playing in Traffic stops at the same refusal of its video). The nine added 2026-10-02 (CNA `3c72165e6`): browser 7 of 9 (Enceladus and the Racing Game Kit start threads), Android 9 of 9 -- 21 of 25 in a browser, 24 of 25 on the emulator Added later on 2026-10-02: Microsoft's Network Prediction, Peer to Peer, Network Game State Management, Memory Madness and Saving Embedded Images (CSX-118/119, CNA CBIND-152; the first two also as SystemLink host and client), Quadtree Terrain, FightingGame, Some 2D RPG, SKraft (CSX-121, CNA CBIND-153), the Forge engine's sample (CSX-122), Sonic 3, HauntedHouse and Disentanglement (CSX-123), NeonVectorShooter, Blackjack (CSX-124, CNA CBIND-154) Petzold's PhreeCell (CSX-125, CNA CBIND-155), asvo, Project Mercury's particle test bench (CSX-126..128) raphaelmun/Xen's Platformer and twin-stick shooter kits (CSX-129) and Asteria's lighting prototype and blend-effect demo (CSX-130; the prototype matches an XNA build under Wine to 2 of 300,000 pixels), klutch's FluidPort, XPF built from source for Windows and Windows Phone with its samples S01, S02, S03 and S05, and from a second GitHub search: Alone, Zombie Run, Shootin, Pyramid Panic, Submarine Destroyer, Super Smash Polls (CSX-131), Kittens & Kobolds, Super Luigi, Square Chase, Risk of Pain, FunGame, Thieves Like Us (identical to XNA under Wine), Reflexio, AlexMeuer's course projects (CSX-133), Apress' book samples (CSX-132), Flight Sim, Heroes of Rock to its menu (CSX-134), Mario3, Spelunky Tiles, the Farseer Physics 3.5 samples and a tile-engine tutorial, MP3Sharp's and SharpMik's players (DynamicSoundEffectInstance, captured output checked), LilyPath's logo (CSX-135) and willcraftia's seven XNA test demos (CSX-136; LiSPSM and TerrainDemo compared with their XNA builds under Wine), cocos2d-x for XNA's test scenes (Windows Phone), BoneAnimation's example, xna-camera-2d's Platformer, a UTS tower defence and *Windows Phone 7 Recipes*' three XNA recipes (CSX-137), ExEn's Marblets, CatGirls and Orientation samples (CSX-138/139), 48 XNA samples of Petzold's *Programming Windows Phone 7*, tiled-xna's example, eight Windows Phone Pong demos and the four games of *XNA 4.0 Game Development by Example* (CSX-141) and gearsvge's GearsDebug with its Radial Assault (CSX-142) |

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
| CSX-113 | A content asset's directories resolve ignoring case, as CNA's native loader does (`models\Cube` for `Models/Cube.xnb`) | done (cna-cs `7780921`); found by the Racing Game Kit |
| CSX-114 | Opt-in `CNA.WindowsFormsCompat`: `Control`/`Form.FromHandle(Window.Handle)` (the game window's form, null otherwise), `FormBorderStyle` applied through CNA's borderless window route (`137f954`, imports 1413 -> 1415), `Opacity` kept but not applied (no window-opacity service), `MessageBox` to stderr answering with its first button (CNA's native dialog needs a live game, and XNA games show it once theirs failed to start); games opt in with `<CnaWindowsFormsCompat>` | done (cna-cs `ff08b93`, `137f954`); found by the Racing Game Kit |
| CSX-115 | XNA `StorageDevice` in every browser bundle: CNA's IDBFS pre-js (`WebStoragePre.js`) is staged beside the archive and linked, which it never was -- every `OpenContainer` refused "Persistent browser storage is unavailable" | done; found by escape-from-enceladus |
| CSX-116 | A game that starts threads, in a browser: a `WasmEnableThreads` bundle links CNA's shared-memory build (`Build-BrowserNative.sh --threads`), takes its frames from Emscripten's main loop on .NET's deputy thread (so the page's input reaches it between frames), and preloads 16 workers -- .NET 11 never starts a thread on a worker it creates after `Main` | done in headless Chromium: escape-from-enceladus, Missile Command, the Racing Game Kit (attract mode), AimingSample; CNA CBIND-151 |
| CSX-117 | A threaded bundle's worker pool scales with the processors its game sees (3 per processor + 8): XNA games size their own threads by `Environment.ProcessorCount`, and Resonance's physics starts two per processor | done in headless Chromium: Resonance loads its level on its thread and plays (32 physics threads on a 16-core host) |
| CSX-118 | ABI 0.41.0: a game thread that waits for a worker runs the calls the worker queued for it (CNA CBIND-152) -- XNA's loading screens join the thread that draws their animation from `Update` | done: Network Game State Management reaches its gameplay screen; integration `AGameThreadThatJoinsAWorkerFromUpdate_RunsTheWorkersDrawing` |
| CSX-119 | A Windows Phone title's Guide keyboard prompt and message box need no GamerServicesComponent (the phone's own UI), and `MediaLibrary.SavePicture` that cannot save throws the phone's `InvalidOperationException` | done: Saving Embedded Images asks for a name and saves (or shows its own "Unable to save image.") |
| CSX-120 | The Windows Forms surface Zelda Oracle's form uses: `System.Drawing.Icon` (.NET's System.Drawing.Common cannot construct one off Windows), `Form.Icon`/`MinimumSize` kept, `Shown` after the first Update (XNA's `Application.Run`, IL), `Focused` from the game's activation, `Activate`, `Application.VisualStyleState`, a process-local `Clipboard` | on branch `zelda-oracle-forms` (cna-cs `23e3900`, cna-cs-samples `851590a`), not merged: Zelda Oracle compiles with it, then stops in `Initialize` at its `user32` window-procedure hook (an `int`-truncated delegate address, x86 only); no running game needs it yet |
| CSX-121 | ABI 0.42.0: `GraphicsDeviceManager.ApplyChanges` in a game's constructor creates the device it then reads, as XNA's does (IL: `ChangeDevice(false)` -> `CreateDevice`; CNA CBIND-153 lends the device before the run) | done: Kermit's SKraft sizes its menu from `Viewport` in its constructor, then plays |
| CSX-122 | `Assembly.LoadFile` as .NET Framework loaded a file, once, by compile-time interception: LoadFile of a file the application loaded, or would load for that name, is that assembly (measured on .NET Framework 4 under Wine, both orders); .NET loads a second copy | done: jacobdufault/forge-sample loads its own `GameLogic.dll` that way and then draws its paddles and balls |
| CSX-123 | XNA's static `Keyboard`/`Mouse`/`GamePad.GetState` answer before a game exists (a field initializer runs before the `Game` constructor): nothing pressed, no controller connected, as XNA's thread key state had no window yet | done: dsplaisted/Disentanglement keeps its previous keyboard state that way and then shows its solver's puzzle |
| CSX-124 | ABI 0.43.0: a game's own thread plays sound effects directly (CNA CBIND-154), as XNA's `SoundEffect.Play` answered on any thread -- a queued play waited forever for a game thread that spins on the worker | done: stpettersens/21's blackjack shuffles on its thread with the shuffle sound, then deals |
| CSX-125 | ABI 0.44.0: `OnActivated` runs with the game's device usable (CNA CBIND-155: activation handlers inside a callback scope), and what a game event's handler throws leaves `Run` at the next Update instead of vanishing in the native callback | done: Charles Petzold's PhreeCell deals and sizes its table in `OnActivated` |
| CSX-126 | A game's own `Range`, `Index` or two-parameter `PriorityQueue` is the one its source means: no .NET Framework had `System.Range`/`System.Index`/`PriorityQueue<,>`, yet on .NET they shadow a game's type through a `using System;` inside its namespace (or make the name ambiguous). A project whose source names one is compiled against copies of System.Runtime/System.Collections with that type internal; it runs on the real assemblies (`CnaNetFxTypeNames=false` turns it off) | done: Project Mercury's 4.0 test bench (`ProjectMercury.Range`) compiles and draws its particle effects |
| CSX-127 | A character its `SpriteFont` lacks fails `DrawString` itself, with XNA's `ArgumentException` (parameter `character`, XNA's message naming it), and the batch goes on: the native text route resolved glyphs at `End`, so the string failed the whole batch there, unnamed (XNA IL: `SpriteFont.GetIndexForCharacter` inside `DrawString`) | done: Project Mercury's failure names `'∞' (0x221e)` at its `DrawString` line |
| CSX-128 | A game prints infinity as `Infinity`/`-Infinity` and negative numbers with an ASCII minus, as the .NET Framework of XNA's Windows (XP to 7) did: .NET's ICU culture data prints `∞` and, for Swedish, Finnish, Norwegian and others, U+2212, which a default SpriteFont (32 to 126) cannot draw. Only those symbols of the user's culture change, for the game's thread and the threads it starts (`CnaNetFxNumberSymbols=false` keeps ICU's). Wine's .NET Framework reads Wine's CLDR-based locale data, so it shows `∞` too and cannot stand in for Windows 7 here | done: Project Mercury's test bench draws `1 / ElapsedGameTime.TotalSeconds`, infinite on its first frame about one start in four, and starts 8 of 8 |
| CSX-129 | A content manager overriding `OpenStream` decides where every asset comes from, built-in types too, as in XNA (its `ReadAsset` reads all through `OpenStream`): `ResourceContentManager`, and a game's own manager reading packed or encrypted content, loaded fonts, textures, sounds, models and effects by path through CNA's loader instead. Those now read through the managed readers, with XNA's `SpriteFontReader` added to them (read order from XNA's IL). Three test fixtures answered every name with their one asset, which XNA's external-reference read loads inside itself without end; they now answer only their own name | done: raphaelmun/Xen's game base loads its fonts from `.xnb` files embedded in its `.resx`; its Platformer and twin-stick shooter starter kits play |
| CSX-130 | An `Immediate` sprite batch draws each sprite when it is given, as XNA's did (native CNA's Immediate batch already did; CNA.NET held every draw to `End`), so effect parameters, device textures or states a game sets between two draws apply to the second only. A string is one submission | done: Asteria's blend demo draws its three clouds with their own blend amounts; the four gallery samples using Immediate batches (Accelerometer, Ship Game, Snow Shovel, Spacewar) requalify as before |
| CSX-131 | A game's `using` of a namespace .NET Framework 4 had and .NET does not (`System.Runtime.Remoting.Messaging`, which Visual Studio added on its own, `System.Management`, ...) compiles, as it did for its author: a directive naming a namespace without types is CS0234 even when unused. For each such namespace the compilation lacks, a generator adds an internal placeholder type; a type the game uses from it is still an error. The list is .NET Framework 4's default assemblies' namespaces that .NET 8's reference pack lacks, counting type forwarders, which make a namespace exist (`System.IO.Ports`, `System.Media` do) (`CnaNetFxNamespaces=false` turns it off) | done: WilliamKluge/SuperSmashPolls compiles and reaches its menu |
| CSX-132 | CNA.PhoneCompat: Windows Phone's `System.IO.IsolatedStorage.IsolatedStorageSettings`, the application settings dictionary WP7 games keep options and high scores in -- `ApplicationSettings`, `Add`/indexer/`Contains`/`Remove`/`Clear`, `TryGetValue<T>`, `Save` -- written with the DataContract serializer (value types listed first) to `__ApplicationSettings` in .NET's isolated store for the application, and saved when the application ends as the phone saved it | done: Windows Phone 7 Game Development's chapter 9 and 10 GameFramework keeps its settings and high scores there; the HighScores and Tombstoning samples run |
| CSX-133 | A failed `Content.Load` is XNA's `ContentLoadException` whatever loads the asset, with XNA's message (`Error loading "x". File not found.` for a missing one): fonts, textures, cube maps, sounds, models and effects go to CNA's own loader, whose failure came out as `CnaException`. `OpenStream` and `ResourceContentManager` now use XNA's messages too | done: AlexMeuer's City Shooter loads `building1`, `building2`, ... until the missing one throws, and catches it |
| CSX-134 | A struct laid out explicitly for XNA's 32-bit processes loads in a 64-bit one: BEPUphysics' Windows build keeps an `Entity` reference at offset 20 of `EntityStateChange` ("an object field at offset 20 that is incorrectly aligned"). When a type's misaligned references all come after its other fields, they move to the next 8-byte slots in the compiled assembly, in order (no code can take the address of a struct holding references); for the game's own assembly after compiling and for the XNA-compiled libraries CSX-099 copies (`CnaSixtyFourBitLayouts=false` turns it off) | done: scotttorgeson/HeroesOfRock's BEPUphysics source and Resonance's x86 BEPUphysics binary (which it had replaced with the phone build to dodge this) both build their physics space |
| CSX-135 | A stock effect draws with the texture on `GraphicsDevice.Textures` when it draws, as XNA's did: Apply binds the effect's texture to its sampler (a null one too), and a texture a game sets on the device after Apply is the one sampled. Native CNA's stock effects drew with their own `Texture` property (CNA CBIND-156) | done: jaquadro/LilyPath's logo demo (its `DrawBatch` applies a texture-enabled BasicEffect with no texture, then sets each brush's texture on the device) draws its lily pad and flower, matching the XNA-rendered image in LilyPath's README to 2,281 of 129,600 pixels (MSAA edges); `EffectDeviceTextureTests` (3) |
| CSX-136 | A compiled effect samples a float texture at full 32-bit precision, as Direct3D 9 did: MojoShader's GLSL ES 3 translation left `sampler2D` at GLSL ES's lowp default, and Mesa read Single/Vector2 textures at fp16 (CNA FX-145, a pinned MojoShader patch) | done: willcraftia's LiSPSM demo blurs its variance shadow map's moments through a SpriteBatch; its shadow was speckled and now matches Microsoft XNA under Wine |
| CSX-137 | A Windows Phone title's `Guide.IsTrialMode` is the phone's license check, which needed no GamerServicesComponent: a title its developer deployed is no trial unless `SimulateTrialMode` makes it one. CNA (rightly, for Windows) answers as XNA's IL does before its dispatcher's first update: true | done: *Windows Phone 7 Recipes*' trial sample, which sets `SimulateTrialMode` only in its Debug build, runs as a full version in Release and as a trial with simulation; `CompatPhoneTrialModeTests` (3) |
| CSX-138 | A drawable component signals `VisibleChanged` and `DrawOrderChanged` only when the value changes, as XNA's setters compare first (IL) and `Enabled`/`UpdateOrder` here already did | done: ExEn's Marblets sets its already visible title screen visible in its game's constructor; its handler starts music before any was loaded and threw. `CompatComponentChangeEventTests` (2) |
| CSX-139 | CSX-128's culture repair covers times: ICU's en-US and other cultures put a narrow no-break space (U+202F) before AM/PM, outside a default SpriteFont's 32-126, where Windows' patterns had a space; the long, short and full time patterns get the space back | done: ExEn's timing test draws `DateTime.ToLongTimeString()` and threw from `DrawString`; `CompatGame_PrintsInfinityAndMinusAsXnasWindowsDid` checks `1:05:07 PM` |
| CSX-140 | A browser build keeps `System.Private.CoreLib` whole (a trimmer root, about 5 MB): the .NET Framework XNA's games and libraries were written for was never trimmed, and they reach its types by reflection; the rest of the framework is still trimmed (switching trimming off altogether once left .NET 11's relinked runtime out of step with its CoreLib) | done: imeteora/BoneAnimation's protobuf-net builds its serializers over `KeyValuePair<string, Skin>`, whose members the trimmer had removed (a `NullReferenceException`); it now draws its hero in headless Chromium, and on the Android emulator, whose build trimmed the same members (`eng/android/CNA.Android.targets` roots CoreLib too) |
| CSX-141 | `BinaryFormatter`, an ordinary .NET Framework 4 API that XNA games and libraries read their maps and saves with, compiles (SYSLIB0011 suppressed) and runs (the runtime switch replaced) for code built against XNA; `<CnaBinaryFormatter>false</CnaBinaryFormatter>` keeps .NET's refusal. Desktop only so far: .NET 9 and later (the browser and Android builds) no longer carry an implementation | done: *XNA 4.0 Game Development by Example*'s Gemstone Hunter loads its levels through its Tile Engine's `BinaryFormatter` and plays; `BinaryFormatterTests` |
| CSX-142 | What a game's `UnloadContent` throws leaves `Game.Dispose`, as in XNA, whose `Dispose` unloads content from the device's `Disposing` event with nothing on the path catching (XNA IL: `Game::Dispose(bool)`, `GraphicsDeviceManager::Dispose`, `GraphicsDevice::~GraphicsDevice`); the native game is released first, `Disposed` is not raised, and native's `Callback` answer is no longer booked as a refused destroy. The binding had swallowed it | done: spectrumbranch/gearsvge's GearsDebug stops its audio thread in `UnloadContent` with `Thread.Abort`, which .NET 5 and later refuse; the refusal vanished and the game's foreground thread kept the process alive with no window. It now ends with that `PlatformNotSupportedException`; `CompatUnloadContentFailureTests` |

### P9 -- portability

Windows/macOS/iOS: architecture only (resolver keeps `.dylib`/`.dll`; iOS planned as static
`__Internal`). Not qualified.

## Ledger

Newest first. Each entry: repos+HEAD, reproduced, root cause, files, tests, commands, results.

### 2026-10-03 (night) -- a second GitHub search: 15 games, two books' samples; CSX-131..134

CNA.NET `18f3dff`, cna-cs-samples `67357b7` (CNA unchanged). The XNA 4.0 project search listed 146
repositories not tried before; the small ones were cloned and triaged by a script
(projects, content processors, fonts, references outside the repository).

1. WilliamKluge/SuperSmashPolls did not compile: an unused `using System.Runtime.Remoting.Messaging;`
   (Visual Studio added such directives) names a namespace .NET has no types in. CSX-131 gives each
   namespace .NET Framework 4's default assemblies had and .NET lacks an internal placeholder when a
   game names it -- counting type forwarders, which make `System.IO.Ports` and `System.Media` exist.
2. Windows Phone 7 Game Development (Apress): chapters 9 and 10 keep settings in
   `IsolatedStorageSettings`; CSX-132 adds it to CNA.PhoneCompat over .NET's isolated store.
   Nineteen samples run; the Accelerometer one needs a sensor the desktop does not have.
3. AlexMeuer's City Shooter loads numbered textures until `ContentLoadException`; CNA's loader threw
   `CnaException`. CSX-133: every failed load is XNA's exception with XNA's message.
4. No defect: Alone (levels at a working-directory path), Zombie Run (a project naming a file in
   another case), Shootin, Pyramid Panic, Submarine Destroyer, Kittens & Kobolds (CodeDom package),
   Super Luigi, Square Chase, Risk of Pain, FunGame (`StorageDevice` save and load), Thieves Like Us
   (identical to an XNA build of its sources under Wine, 0 of 384,000 pixels), Reflexio (Windows level
   paths linked; its missing AssemblyInfo.cs compiled without). Blocked: Mythology (a source file its
   project omits), Asteria's game (System.Xaml/WPF), SeedWorld, BulletXNA demos, Isosurface,
   MunchKlone (MySql), Project Squared (Consolas).
5. Browser (CNA `05b9a435d`, archives rebuilt): Mercury, Xen's kits, Asteria's demos, FluidPort, XPF
   S01/S05, PhreeCell, NeonVectorShooter, two shader tutorials, three 2dGPfG samples, Alone and
   Pyramid Panic reach their screens in headless Chromium. `browser-sample.sh`/`android-sample.sh`
   name a nested game by both directories and carry a library's own dependencies (XPF's Rx).

6. CSX-134: BEPUphysics' Windows build lays out a struct for 32-bit processes (a reference at offset
   20); scotttorgeson/HeroesOfRock's physics space could not be built. The compiled layout is
   realigned; Resonance now runs on the x86 BEPUphysics its Windows project names. Heroes of Rock
   stops at its debug overlay's Windows performance counters; its MP3 sound effects are labelled
   stand-ins (`scripts/sound-standin.sh`). wontonst/flightsim flies (its process outlives its window:
   Henge3D's foreground workers, as on Windows).
7. Android (CNA `05b9a435d`, x86_64 emulator): Sonic 3, Disentanglement, HauntedHouse, Mercury, Xen's
   Platformer, Pyramid Panic, Cosmic Rocks III, PhreeCell, Super Luigi, FluidPort, XPF S05 and Alone
   all draw; the emulator was stopped. Browser: also XPF S01, PhreeCell, Blackjack and asvo (threaded;
   asvo needs over a minute to voxelize), Cosmic Rocks III, High Scores, Kittens & Kobolds.

Tests: CNA.NET framework 653/653, XnaCompat 312/312, integration 247/247.

### 2026-10-03 (later) -- Project Mercury, Xen's starter kits, Asteria's demos, the 2D graphics book; CSX-126..130

CNA.NET `a287d4e`, cna-cs-samples `c9b53e8` (CNA unchanged, `765a000f6`).

1. alaskajohn/2dGPfG, the fourteen XNA 4.0 samples of *2D Graphics Programming for Games*, built
   from their shared content project, run; no defect found.
2. Project Mercury's 4.0 test bench did not compile: its `ProjectMercury.Range`, named under a
   `using System;` inside its namespace, bound `System.Range`. CSX-126 compiles a project that names
   `Range`, `Index` or `PriorityQueue` against reference copies where those types are internal.
   Then it ended about one start in four from `SpriteBatch.End`, naming no character: its first
   frame's elapsed time can be zero, its frame rate `1 / 0` printed ICU's "∞", and its font holds
   32 to 126. CSX-127 refuses such a character in `DrawString`, with XNA's exception; CSX-128 keeps
   the ASCII "Infinity" and minus that XNA's Windows printed (Wine's .NET Framework uses Wine's
   CLDR data, so it is no witness for Windows 7 here). 8 of 8 starts now. Its Demo2 overflows its
   own 10000-vertex array, which XNA's `SetData` check (`ValidateCopyParameters`, IL) refuses too.
3. raphaelmun/Xen's game base loads its font through a `ResourceContentManager` from the `.xnb`
   its `.resx` embeds, and CNA.NET sent fonts to CNA's loader by path. CSX-129: a manager whose
   `OpenStream` is overridden reads built-in types from it through the managed readers, XNA's
   `SpriteFontReader` among them now. Its Platformer and twin-stick shooter play. The Platformer's
   sounds are `.wma`, which WmaImporter cannot decode under Wine; they are byte-identical to
   Microsoft's Platformer starter kit's, so `build-xna-content.sh --official` takes that kit's
   retained XNA output.

4. Bryan-Legend/asteria's blend-effect demo drew its three clouds alike: it sets its effect's second
   texture and blend amount before each draw of an Immediate batch, and CNA.NET held the batch to
   `End`, so all drew with the last values (native CNA's Immediate batch already drew at once).
   CSX-130 submits each draw of an Immediate batch when given. Its lighting prototype matches an XNA
   build of its own sources (Framework `csc` under Wine, HiDef profile resource embedded) to 2 of
   300,000 pixels; the blend demo now draws as that XNA build does.

5. No defect found: klutch/Box2DFluid's FluidPort (Farseer 3.3.1 built from the source
   HauntedHouse's repository ships, as its own DLL reference points outside its repository) and
   Red Badger's XPF, built from source for Windows and Windows Phone, with four of its samples (S05's
   card turns over through a binding). Blocked: the Asteria game itself (`System.Xaml`
   `XamlServices` save files, WPF imaging; its Xbox project no longer compiles), SeedWorld (a Nuclex
   pipeline extension it does not ship).

Tests: CNA.NET framework 653/653, XnaCompat 310/310, integration 244/244. Every gallery sample and
game project rebuilt against CSX-126's targets; the four gallery samples with Immediate batches
requalify as before.

### 2026-10-03 -- three more games; CSX-124/125, CNA CBIND-154/155 (ABI 0.43.0, 0.44.0)

CNA `ed885a8ae`, CNA.NET `a5a5c7a`, cna-cs-samples `6714bf2`.

1. stpettersens/21's blackjack never drew a frame: its dealer's thread plays the shuffle sound while
   the game thread spins on `IsAlive` -- not a wait, so CSX-118 never ran the worker's queued call
   and both threads ran forever. XNA's `SoundEffect.Play` answered on any thread. CNA CBIND-154 lets
   the two fire-and-forget play routes look their effect up from any thread (the mixer serializes;
   every other route keeps the rule); CSX-124 admits 0.43.0.
2. Charles Petzold's PhreeCell (*Programming Windows Phone 7*) drew nothing: it deals and sizes its
   table in `OnActivated` from the viewport, and CNA raised Activated outside every callback scope,
   so the device borrow was refused -- and CNA.Framework's event bridge swallowed the exception.
   CNA CBIND-155 raises Activated/Deactivated in a callback scope with the context lease; CSX-125
   admits 0.44.0 and rethrows a game event handler's failure at the next Update, so it leaves
   `Run` as XNA's did. The six other games that handle activation still run.
3. NeonVectorShooter (thiagoromam) plays with its bloom, no defect. In headless Chromium (CNA archive
   `90b553dc1`): Sonic 3, Disentanglement and HauntedHouse reach their first screens; SKraft starts
   map threads, so it runs as a threaded bundle (`--threads`) to its menu; the Forge sample needs UDP.
4. Triaged and recorded: zelda-platformer (a tech demo whose map needs a prebuilt pipeline DLL),
   XNA-Game-project (Kinect, Speech SDK, missing sources), Team7 (missing sources and art), xnaheist
   (DebugView's Arial font), Petzold's other phone demos (runnable, not games).

Tests: CNA `^CApi` 119/119, `^(Game|Runtime)` 297/297; CNA.NET framework 653/653, XnaCompat
304/304, integration 239/239, GamerServices 24/24, `Verify-Abi.sh` 0 mismatches. Desktop gallery
requalification on 0.44.0 (`scripts/requalify.sh`, `/rv/tmp/cs-samples/requalify-0.44`): 84 rows,
every build, run and exit status the 2026-10-02 baseline's (`requal-20261002`), the same six rows
not exiting on Escape as there. Then asvo (a software voxel renderer on worker threads, XNAnimation
pipeline: `build-xna-content.sh` now signs an extension with its project's key and embeds its
`.resx`) and petriw's seven XNA 4.0 shader tutorials ran, no defect found.

### 2026-10-02 -- GitHub's XNA 4.0 projects: four more run; CSX-122, CSX-123

CNA.NET `02b740f`, cna-cs-samples `14143de` (CNA unchanged, `938a22069`). A GitHub code search
for XNA 4.0 project files (`XnaFrameworkVersion>v4.0<`, `XnaPlatform>Windows<`) listed 195
repositories; the games among the most-starred were cloned and triaged (13 trees, two agents).

1. jacobdufault/forge-sample drew nothing: it `Assembly.LoadFile`s its own `GameLogic.dll` and then
   finds renderers by the types its references bind to. Measured on .NET Framework 4 under Wine (a
   two-assembly probe built by Framework's `csc`): LoadFile of the app's own file is the referenced
   assembly, in either order; a copy elsewhere is a second one. .NET 8 makes a second one always.
   CSX-122 intercepts the game's `Assembly.LoadFile` calls (as CSX-110 does `List<T>.ForEach`):
   a file already loaded is that assembly, a trusted-platform file loads into the default context.
   Its log4net and Json.NET 5 also need `System.Configuration.ConfigurationManager` and
   `System.Security.Permissions` packages (Forge's swallowed failure was found by turning its
   log4net on through an app setting, not by changing the game).
2. Disentanglement read `Keyboard.GetState()` in a field initializer, before any CNA game exists:
   CSX-123 answers XNA's static input classes with the empty state until then.
3. Sonic 3 (JonathanDechelle) plays Angel Island at its 2014 state; its 2017 tip never starts a
   level. HauntedHouse runs Krypton's lighting on the XNA output its repository ships; its TiledLib
   compiles against Game Studio's content pipeline, now a compile-time reference only, and the
   games stopped taking the SDK's default `None` items, which had copied that assembly from a
   library subproject's `obj/`. One CNA warning misfires there: Sonic's `Jump` sound is PCM16 that
   XNA built, with byte entropy 7.985, above the 7.9 at which CNA calls a buffer compressed; it
   only logs, and byte statistics cannot tell loud noise from compressed data.
4. Recorded as not running: Design Patterns Game (MEF constructs 21 `Game` objects; CNA runs one
   game per process), Kodu Game Lab (a Windows Forms host), Jxqy HD and Tactile Engine (data or
   sibling repositories they do not ship), five games whose sprite fonts are Windows fonts, and
   XNA 3.x or MonoGame projects.

Tests: CNA.NET framework 651/651, XnaCompat 304/304, integration 236/236, GamerServices 24/24.

### 2026-10-02 -- SKraft plays; CSX-121 and CNA CBIND-153 (ABI 0.42.0); Zelda Oracle on a branch

CNA `938a22069`, CNA.NET `834e7ec`, cna-cs-samples `4d66fd6`. The five Microsoft samples of the
previous entry also run as a SystemLink host and a joining client in two processes (Network
Prediction, Peer to Peer), and reach their first screens in a browser and on the Android emulator;
where they stop there is recorded in cna-cs-samples `games/README.md` (a blocking Guide `End*` in a
single-threaded page; Network Game State Management's `Thread.Join` on Mono, which does not consult
the SynchronizationContext CSX-118 relies on). Quadtree Terrain, FightingGame and Some 2D RPG run
unchanged with no defect found.

1. Kermit/SKraft (`2480755`, content by XNA's BuildContent with its own model processor, Reach)
   threw a NullReferenceException in its constructor: it calls `ApplyChanges` there and sizes its
   menu from `GraphicsDevice.Viewport`. XNA's `ApplyChanges` with no device creates it (IL:
   `ChangeDevice(false)` -> `CreateDevice`; FNA's too), CNA.NET left it null until the run, and
   native lent the device only inside a callback. CNA CBIND-153 lends it before the run on the
   creating thread (ABI 0.42.0, a changed rule, 0 breaking differences); CSX-121 wraps it in
   `ApplyChanges`. SKraft then plays: instanced cubes through its own shader, mouse look, its day
   cycle, Quit saving its sectors. Its world is read from a Windows path its repository leaves
   empty; the run copies the shipped sectors to the name Linux reads.
2. Zelda Oracle (`92be3a1`; the tip adds a file that compiles nowhere) needed Windows Forms beyond
   CSX-114 -- `System.Drawing.Icon`, which .NET's System.Drawing.Common cannot construct off
   Windows, `Form.Shown`/`Focused`/`Icon`/`MinimumSize`/`Activate`, `Application.VisualStyleState`,
   `Clipboard` -- and then stops in `Initialize` at its `EventInput` hook: `SetWindowLong(GWL_WNDPROC,
   (int)Marshal.GetFunctionPointerForDelegate(...))`, compiled to `conv.i4`, so x86 Windows only.
   AutonomousCar uses the same hook. CSX-120 is kept on branch `zelda-oracle-forms` (cna-cs
   `23e3900`, samples `851590a`) until a running game needs it.
3. Ten more local trees triaged: Voxeliq's XNA client needs Calibri (three sprite fonts), Pokémon
   Azure Lua DLLs it does not ship, Old School Adventure is MonoGame now; AngryTanks, Flotilla, XNA
   Street Fighter and Infiniminer are XNA 3.x.

Tests: CNA `^CApi` 119/119, C API gates 9/9; CNA.NET framework 651/651, XnaCompat 299/299,
integration 235/235, GamerServices 24/24, `Verify-Abi.sh` 1416 prototypes and 0 mismatches, ABI
fixtures 2 accepted and 10 rejected. Browser and Android bundles still carry the 0.41.0 archives and
need CNA rebuilt for them before their next run.

### 2026-10-02 -- five Microsoft samples outside the gallery; CSX-118/119, CNA CBIND-152

CNA `c77f983b1` (+ the GS-AUDIT merge `46d55fa26` before it), CNA.NET `df752a2`, cna-cs-samples
`c491dbb`. Microsoft XNA 4.0 samples with original trees and XNA-built content in cna-samples but no
C++ port run as `games/` (the owner agreed): Network Prediction, Peer to Peer, Network Game State
Management, Memory Madness, Saving Embedded Images. All five compile unchanged and play on the
Linux desktop. What they found:

1. Glue (cna-cs-samples): the games glue took only `Compile` items from a game's project; it now also
   embeds its `.resx` under XNA's manifest name and copies the Content/None files its build copied
   (NGSM's `MissingManifestResourceException`, Saving Embedded Images' missing `GameProjectImage.jpg`).
2. CNA CBIND-152 / CSX-118 (ABI 0.41.0): NGSM's `LoadingScreen` joins, from `Update`, the thread
   that draws its loading animation through the `GraphicsDevice`; that thread's calls waited for the
   next `Update`, so both waited forever (a CDP-free diagnosis this time: `gdb` showed the game
   thread in coreclr's wait, the source showed the `Join`). A probe confirmed .NET calls
   `SynchronizationContext.Wait` for `Thread.Join`, wait handles, `Monitor.Wait` and
   `ManualResetEventSlim` on Linux; the game thread's context now runs both queues between 2 ms
   slices of every wait. Drawing from the worker happens while the game thread waits, not while it
   computes (XNA ran it concurrently).
3. CSX-119: a Windows Phone title's Guide keyboard prompt and message box needed no
   GamerServicesComponent (Saving Embedded Images has none; Windows XNA needs the dispatcher, its
   IL shows); `MediaLibrary.SavePicture` that cannot save throws the phone's
   `InvalidOperationException` (Windows XNA throws `NotSupportedException` always).

Input lessons: xdotool `key` is a tap a polled Guide misses (hold Escape); `type` swallows the rest
of a chained command line; the Guide ignores input while it signs a player in. Known, unchanged: a
game blocked in a Guide `End*` wait (CNA's modal frames) does not see CSX-084's SIGTERM request
until the Guide closes. Threaded browser subset: 16 representative gallery rows pass as
multithreaded bundles (`browser-requalify.sh --threads`). Tests: Framework 650/650, XnaCompat
299/299, integration 234/234, GamerServices 24/24, api-compat 0; CNA `^CApi` 119/119, C API gates 9/9.

### 2026-10-02 -- CSX-115/116/117: games that start threads, in a browser; CNA CBIND-151

CNA `cde2251fa`, CNA.NET `e550614`, cna-cs-samples `fe1d8d6`. Five real games start threads, which a
single-threaded bundle refuses (`PlatformNotSupportedException`): HeliumBiker, Resonance's level
load, Escape From Enceladus, the Racing Game Kit; Missile Command too. A `WasmEnableThreads` bundle
(`scripts/browser-sample.sh games/<Game> --threads`, CNA from `Build-BrowserNative.sh --threads`)
now runs them. What it took, in the order found:

1. CNA CBIND-151, four defects of a game thread that is a worker (.NET's deputy runs `Main`): the
   shared-memory link's initial memory; a worker's proxied WebGL context presented only by
   `emscripten_webgl_commit_frame()` (SDL patch 0004 -- the canvas stayed black); the worker's
   `GLctx` stand-in without `getExtension`; `StorageDevice` reading the IDBFS flag in the worker's
   realm.
2. CSX-115: `WebStoragePre.js`, which mounts IDBFS, was never linked into any browser bundle; every
   `OpenContainer` refused. Found by Enceladus; affects single-threaded bundles as well.
3. CSX-116, the loop: a JSImport from the deputy runs on the page's thread, which may not call C#
   back synchronously, so the first threaded loop blocked on the deputy. It drew, but had no keyboard.
   The page hands SDL's input callbacks to the thread that registered them as queued calls, run when
   that thread returns to its event loop. Draining them from `SDL_PumpEvents` did nothing: the
   deputy's whole loop ran inside the proxied call to `Main`, a system-queue task, and Emscripten's
   recursion guard returns from a nested drain. Frames now come from `emscripten_set_main_loop_arg`
   on the deputy (an `[UnmanagedCallersOnly]` frame), `Main` returns as in a single-threaded bundle,
   and the events arrive between frames (a mailbox-check count went from 1 to 1 + one per event).
4. CSX-116, the pool: the Racing Game Kit hung after frame 1. Pausing every worker through CDP
   (`Debugger.pause` per auto-attached worker, a relink with `WasmNativeStrip=false`) showed the game
   thread in `Thread.Start` -> mono `create_thread` -> `sem_wait`, and the page's Worker objects
   showed the new thread assigned to a worker with `loaded=false`. .NET 11 RC1's `getNewWorker`
   returns the first unused worker whose `loaded` is set, which nothing sets with this Emscripten,
   so it falls back to the worker it has just created; a thread given such a worker never runs.
   Reproduced with no CNA (`build-consumer/browser/ThreadStartProbe`, gitignored: `Main` starts
   blocking threads one by one): 3 start on the default pool of 7, 12 on 16. Upstream, not fixed here.
5. CSX-117: XNA games size their threads by `Environment.ProcessorCount` (the browser's
   `hardwareConcurrency`); Resonance's level builds a BEPUphysics space with two threads per
   processor (74 threads in its desktop run on 16 cores, 32 of them physics), and hung on 16 or 32
   workers. The default page preloads `max(16, 3 * processors + 8)`; a worker costs about 5 MB in
   Chromium (649 MB with 16, 889 MB with 64) and startup a few hundred ms. `DOTNET_PROCESSOR_COUNT`
   is honoured by this runtime but not used: the game sees the browser's own count.

Results, headless Chromium (SwiftShader): Escape From Enceladus -- title and its three save slots
from IndexedDB; Missile Command -- Space starts a game; Resonance -- its threaded level load, the
arena, ArrowUp moves; HeliumBiker -- its CONNECT screen, as on the desktop; the Racing Game Kit --
its attract mode at about 0.65 fps (each GL call of the worker is a round trip to the page's thread;
its menus test a press while drawing, and at that rate XNA's fixed-step catch-up runs several updates
per draw, so they never see one); AimingSample, threaded and single-threaded -- held ArrowRight moves
the cat, Escape ends the run. The single-threaded browser requalification passes 84/84 in one run
(`/rv/tmp/cs-samples/browser-requal-20261002-st/`, CNA with CBIND-151's changes; NetRumble, failing
in the CSX-108 partial run, passes); against the earlier passes only animated rows moved. Framework
646/646, XnaCompat 297/297, integration 233/233.

Not done: a threaded bundle's WebGL runs proxied (an OffscreenCanvas on the deputy would need .NET to
hand the canvas to the thread it creates); interactive Chrome with a GPU; browser audio.

### 2026-10-02 -- CSX-114's border style applied; CNA FX-144; the Windows Forms games re-checked

`Form.FormBorderStyle` now reaches the game window: CNA.Framework's `GameWindow.IsBorderlessEXT` binds
`cna_game_window_get/set_is_borderless_ext` (present in 0.40.0; 1415 imports, `Verify-Abi.sh` 0
mismatches, native ABI fixtures regenerated, `CnaAbiTests` tripwire 1413 -> 1415), and the Racing Game
Kit's `FormBorderStyle.None` removes its border as under XNA. `GameWindow_BorderlessRoundTrips` and the
Windows Forms integration test cover it; integration 233/233, Framework 646/646, XnaCompat 297/297,
api-compat 0. CNA FX-144 (`47b5f7665`): the one OPENGLES3 compiled-effect test that aborted
(`FlippedSourceRetainsFormatAndFullFloatPrecision`) called meta-gl through the test binary's own,
never-initialised copy (CnaTests links meta-gl statically, apart from `libcna.so`'s); it now
initialises it from the renderer's loader and passes -- `ctest -R EasyGLCompiledEffect` 685/686, the one
left being the vertex-texture LOD bias OpenGL ES 3 cannot express. The Windows Forms-blocked games,
re-checked against CSX-114: none is unblocked -- Flux uses `System.Drawing` only in a helper it never
calls (the official package would compile it) but its fonts Fabada and Origin are not in its repository;
infinecraft needs a Neoforce library its repository lacks; Mannux P/Invokes `winmm.dll`; Minor
Destruction stops at .NET 7's `BitConverter.GetBytes(sbyte)` ambiguity.

### 2026-10-02 -- the nine newest games in a browser and on Android; CNA CBIND-149/150

Browser (headless Chromium, CNA `3c72165e6` archive): Missile Command, __Defense, Super Mario World,
the Zelda clone, Spineless reach their titles at once. Mahjong's generated project lacked its
ConfigurationManager package and the Racing Game Kit's its CNA.WindowsFormsCompat: the browser and
Android generators now carry `<CnaWindowsFormsCompat>` and the game project's `PackageReference`
items (cna-cs-samples `4994508`); Mahjong then shows its menu. Bubble Bound stopped on "Cannot present
while render targets are bound" after "native GL errors were pending before MRT setup: InvalidEnum".
Two WebGL 2 gaps, each reproduced and fixed in CNA EasyGL with a test: `glEnable(GL_SAMPLE_MASK)`
issued because a `glSampleMaski` entry point resolved (CBIND-149 `d459f42aa`, reproduced natively
with `MESA_GLES_VERSION_OVERRIDE=3.0`; `EasyGL_Es30SampleMask`), and `GL_TEXTURE_SWIZZLE_*` on a
Single/Vector2 render target, which Chromium named as "texParameter: invalid parameter name"
(CBIND-150 `3c72165e6`; `EasyGLProfile.OnlyDesktopGlAndGles3SwizzleTextures`). Bubble Bound then
reaches its title. Escape From Enceladus and the Racing Game Kit start threads (`new Thread(...).Start()`)
and stop with `PlatformNotSupportedException` -- single-threaded WebAssembly, like HeliumBiker.
Android (x86_64 emulator, CNA `3c72165e6`): all nine run, including both threaded games. A headless
emulator left from 2026-10-01 was found using ~12 cores and stopped; the runs now end by stopping it.

### 2026-10-02 -- CNA FX-143: volume samplers on GLSL ES 3

The handoff's next step, taken: GLSL ES 3.00 gives `sampler3D` no default precision, so every
MojoShader GLSL ES 3 shader with a volume sampler failed to compile on OPENGLES3 and WEBGL2 -- any
HiDef game with a `Texture3D` on GLES or in a browser. CNA `d9d599263` appends a pinned MojoShader
patch declaring `precision highp sampler3D;`; five failing CNA compiled-effect tests pass
(`ctest -R EasyGLCompiledEffect` 679 -> 684 of 686). Eight deterministic desktop gallery rows with
compiled effects are pixel-identical before and after; SpriteEffects' lower-right quadrant moves by at
most 2/255 per channel. `^CApi` 119/119. Left: vertex-texture LOD bias on GLES 3 (no sampler LOD bias
there) and `FlippedSourceRetainsFormatAndFullFloatPrecision` aborting.

### 2026-10-02 -- the Racing Game Kit races; its "lost input" was the harness

The input that seemed not to reach the Racing Game Kit after its full-screen switch was the capture
harness's: xdotool's 0.3 s key press fell between two polls of a game that takes longer than that to
draw a frame under llvmpipe (its `Input.KeyboardSpaceJustPressed` compares two consecutive polls, as
XNA games do). Windowed at 1024x768 from a seeded `RacingGameSettings.xml`
(`<capture home>/.local/share/game/RacingGame/Player1/`), Space held 2 s opens the main menu; three
more held presses and the up arrow start a race on the Advanced track -- lap 1/3, 62 MPH in 2nd gear,
HUD, post-processing, no render-loop error (`/rv/tmp/cs-samples/games-racing-race/`). In full screen
(the default) a 3 s press opens the menu as well (`games-racing-fullscreen-menu/`); SDL's "Time out
elapsed after mode switch ... reverting" on Xvfb is harmless. No CNA or CNA.NET change. Run by hand
from `games/RacingGame/bin/Release`: XNA's `AudioEngine` resolves `Content\Audio\...` with
`Path.GetFullPath` (XNA IL), i.e. against the working directory. Not compared with the C++ port's
frames; not tried in a browser or on Android.

### 2026-10-02 -- the XNA 4.0 Racing Game Kit; CSX-113, CSX-114, CNA FX-142

Source `/rv/tmp/XNAGameStudio/Samples/XNA-4-Racing-Game-Kit-master/RacingGameWindows1/RacingGame/RacingGame`
(identical to cna-samples SAMPLE-152 `xna4-original`), unchanged; content is SAMPLE-152's
`evidence/xna4-authentic-build/Debug/Content`, its own content project built by XNA 4.0 on Windows.
cna-cs-samples `games/RacingGame` (`2c3058c`). In order of discovery:

1. It does not compile on Linux: `System.Windows.Forms` (`Form.FromHandle(Window.Handle)` to hide its
   form while loading; `MessageBox` when it cannot start). CSX-114 adds the opt-in subset.
2. `ContentLoadException` for `Content\models\Cube`: XNA's build wrote `Models/Cube.xnb`. Native CNA
   resolves every path component ignoring case; the managed model probe re-cased only the file name.
   CSX-113 (`XnaContentPathCaseTests.ADirectoryThatDiffersOnlyInCaseResolvesToo` fails without it).
3. "Begin cannot be called again" from its render loop, which catches and logs to
   `IsolatedStorage/.../Log.txt`: the first logged error was a compiled-effect link failure on
   OPENGLES3, "fragment shader input `io_10_0' has no matching output". `MESA_GLSL=dump` showed a
   centroid/plain name mismatch, not an unmatched input: MojoShader's link-time centroid rule ignored
   the pixel shader's Shader Model, so a ps_1_x pass reading COLOR0 met a vertex shader writing
   `cna_centroid_10_0`. CNA FX-142 (`fb5cb3ba2`, a final pinned MojoShader patch); CNA's own
   `EasyGLCompiledEffectTest.AuthenticXna4LegacyPassBindsSamplersAndSurvivesClone`, which loads this
   game's normal-mapping effect, failed before it and passes after.

Then it loads for about 40 s, switches to full screen (its saved default) and runs its attract mode --
the city and mountain track flown with the car, shadows, "Press START to continue" -- with no error
in its own log (`/rv/tmp/cs-samples/games-racing-final/`). **Not verified**: its menu and a race. Space
(its continue key), a click and Enter did not reach it after the full-screen switch under
capture-sample (SDL logs "Time out elapsed after mode switch ... no window becoming fullscreen;
reverting" and the original window id disappears); focusing the window found by name did not help.
Whether that is the harness (Xvfb without a window manager) or CNA's full-screen path is not known.
Not compared with the C++ port's frames (its reference is OPENGL33) and not tried in a browser or on
Android.

Also found: CBIND-148's new test raised SDL references past the non-production budget and the next
reconfigure failed; CNA `836b796ae` uses `IPlatformMouse::SetGlobalPosition` and budgets the rest.
`ctest -R EasyGLCompiledEffect` on OPENGLES3: 679/686; the seven failures are identical with FX-142
removed (five `sampler3D` without a precision qualifier, the two shared vertex-sampler contracts,
`FlippedSourceRetainsFormatAndFullFloatPrecision` aborting).

### 2026-10-02 -- Spineless and Mahjong; CNA CBIND-146/147/148

cna-cs-samples `games/Spineless` (gnomicstudios/GGJ13 @ `ae0934b`) and `games/Mahjong` (Jomata/Mahjong
@ `bc81398`), both unchanged. Spineless's `ContentManager.Load<SoundEffect>` threw
`ArgumentOutOfRangeException("sampleRate")`: XNA's ADPCM processor wrote three of its sounds above
48000 Hz (up to 48084), XNA's reader hands that format to XAudio2 (1000-200000 Hz), and CNA's reader
had inherited `FromBuffer`'s 8000-48000 check -- CNA CBIND-146 (`51d9c84cc`), reader test fails
without it. Mahjong's menu came up on its second entry with no input; a probe game logging
`Mouse.GetState` and `Window.ClientBounds` showed two divergences, both settled by XNA's IL:
`ClientBounds` is `PointToScreen(Point.Empty)` plus `ClientSize`, and CNA zeroed the position
(CBIND-147, `e6f9d5384`); `Mouse.GetState` is `GetCursorPos` made client-relative, and CNA's SDL3
backend reported SDL's last pointer-event position, clamped at the window edge (CBIND-148,
`6e273cd89`, now SDL's global state minus the window position on X11/Windows/macOS, as FNA). Each
has a native test that fails without it. The menu's own selection still follows the pointer the
game saw before the capture harness moved its window -- the game hit-tests the previous frame's
position, which XNA would report the same. The harness's move could be undone by the game's
resize (cna-cs-samples `eb005a4` repeats it until it holds). Mahjong then deals on a click on Play.

Results on CNA `6e273cd89`: CNA.NET integration 231/231, Framework 645/645, XnaCompat 290/290;
CNA runtime 206 + 2 platform-refusal skips, platform/mouse/window/`^CApi` groups 417 + 476
(`EasyGL_RealWindowResize` was a stale 2026-09-30 binary; rebuilt, passes). Under `-j8`, three
`^CApi` runs each lost one to three different timing-sensitive smokes that pass alone
(`RuntimeComponentsSmoke` stage 8 asserts one update per fixed-timestep frame).

Desktop gallery requalification on CNA `22b30b30e` / CNA.NET `1326161` (`/rv/tmp/cs-samples/
requal-20261002b`, clean rerun `-c`): 84 rows, statuses as the baseline, every static row
pixel-identical to the baseline C# capture.

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
