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
| CSX-031 | Platform layout decision (desktop/browser/android projects) | todo |

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
| CSX-053.. | One task per eligible sample: unchanged source, XNB content, Debug/Release, run, controls, clean exit, pixel comparison | todo |

### P7 -- browser

| ID | Task | Status |
| --- | --- | --- |
| CSX-060 | Experiment: .NET wasm + `wasm-tools` workload + static CNA via `NativeFileReference` | done: .NET 11 (emscripten 6.0.3, libc++ 21) links CNA's WebGL2 archives; a C# XNA game draws in headless Chromium. .NET 10/8 pin emscripten 3.1.56/3.1.34, whose libc++ 17 has no `std::jthread` |
| CSX-061 | Frame-stepped game loop on browser event loop | doing: CNA CBIND-134 (ABI 0.38.0, `cna_game_run_frame_ext`) admitted and imported; managed `Game.Run` on the browser next |
| CSX-062 | Real Chrome run of an unchanged XNA-style game: rendering, input, lifecycle, reload | todo |
| CSX-063 | Browser sample corpus | todo |

### P8 -- Android

| ID | Task | Status |
| --- | --- | --- |
| CSX-070 | Toolchain check (`~/Android/Sdk`, NDK, .NET android workload), per-ABI CNA build | todo |
| CSX-071 | Android host + sample, emulator run: lifecycle, graphics recreation, touch, content, audio | todo |

### P5b -- behavioural gaps found on the way

| ID | Task | Status |
| --- | --- | --- |
| CSX-080 | Facade exception types: map native refusals to the XNA exception the same call throws | doing: ABI 0.37.0 names the canonical exception (CNA e9dd5d879); GS/Net/Avatar/Guide, PhoneCompat and the audio/media/content boundaries samples catch around re-raise it; other facade calls still leak CnaException |
| CSX-082 | Callback exceptions unwind out of `Run`/`Components.Add` with their own type and stack | done (741e441) |
| CSX-081 | `GraphicsDeviceManager` default profile from the `Microsoft.Xna.Framework.RuntimeProfile` resource (XNA IL `ReadDefaultGraphicsProfile`), plus MSBuild glue embedding it from `<XnaProfile>` | done; samples' Directory.Build.targets import + per-sample XnaProfile is part of CSX-050 |
| CSX-083 | Device state cache coherent with what native SpriteBatch applies; BlendFactor/MultiSampleMask/ReferenceStencil dirty the state as XNA's setters do | done |
| CSX-084 | SIGTERM/SIGINT during `Run` end the game through its own exit instead of the runtime's `exit()` racing the game thread | done |
| CSX-085 | A `CNA_Handle` is held as the 64-bit value it is, not narrowed to pointer width (WebAssembly, any 32-bit target) | done |
| CSX-086 | `Game` lifecycle in XNA's order: `BeginRun` after Initialize/LoadContent, `EndRun` after `Exiting`, `Disposed` raised | done |

### P9 -- portability

Windows/macOS/iOS: architecture only (resolver keeps `.dylib`/`.dll`; iOS planned as static
`__Internal`). Not qualified.

## Ledger

Newest first. Each entry: repos+HEAD, reproduced, root cause, files, tests, commands, results.

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
