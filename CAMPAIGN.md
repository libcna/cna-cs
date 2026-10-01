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
| CSX-030 | Template builds/runs against migrated binding; fresh `dotnet new` generation verified (dev + package) without hidden absolute paths | todo |
| CSX-031 | Platform layout decision (desktop/browser/android projects) | todo |

### P5 -- XNA surface expansion

| ID | Task | Status |
| --- | --- | --- |
| CSX-040 | Inventory current CNA GamerServices/Guide/Avatar/Net C API vs XNA 4.0 metadata | todo |
| CSX-041 | `Microsoft.Xna.Framework.GamerServices` runtime profile (Gamer, SignedInGamer, Guide, GamerServicesComponent, profiles, achievements, leaderboards) | todo |
| CSX-042 | Avatar types (AvatarDescription, AvatarAnimation, AvatarRenderer, ...) | todo |
| CSX-043 | `Microsoft.Xna.Framework.Net` (NetworkSession, AvailableNetworkSession, NetworkGamer, LocalNetworkGamer, PacketReader/Writer) | todo |
| CSX-044 | Missing C API routes added to CNA with pure-C tests where native C++ already has behaviour | todo |
| CSX-045 | Separate opt-in phone compatibility assembly (`Microsoft.Devices` etc.) only where samples need it | todo |

### P6 -- samples (`cna-cs-samples`)

| ID | Task | Status |
| --- | --- | --- |
| CSX-050 | Requalify existing checked-in C# rows against migrated binding | todo |
| CSX-051 | Generated inventory: gallery (`samples.libcna.com`) x C++ evidence (`/rv/tmp/samples`) x original source (`/rv/tmp/XNAGameStudio/Samples`) | todo |
| CSX-052 | Update obsolete cna-cs-samples policy (read-only CNA, stop-for-owner) | todo |
| CSX-053.. | One task per eligible sample: unchanged source, XNB content, Debug/Release, run, controls, clean exit, pixel comparison | todo |

### P7 -- browser

| ID | Task | Status |
| --- | --- | --- |
| CSX-060 | Experiment: .NET 8 wasm + `wasm-tools` workload + static CNA (`cna_c_api_wasm`) via `NativeFileReference`/`__Internal` vs installed emsdk | todo |
| CSX-061 | Frame-stepped game loop on browser event loop | todo |
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
| CSX-080 | Facade exception types: map native refusals to the XNA exception the same call throws | todo |
| CSX-082 | Callback exceptions unwind out of `Run`/`Components.Add` with their own type and stack | done (741e441) |
| CSX-081 | `GraphicsDeviceManager` default profile from the `Microsoft.Xna.Framework.RuntimeProfile` resource (XNA IL `ReadDefaultGraphicsProfile`), plus MSBuild glue embedding it from `<XnaProfile>` | todo |

### P9 -- portability

Windows/macOS/iOS: architecture only (resolver keeps `.dylib`/`.dll`; iOS planned as static
`__Internal`). Not qualified.

## Ledger

Newest first. Each entry: repos+HEAD, reproduced, root cause, files, tests, commands, results.

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
