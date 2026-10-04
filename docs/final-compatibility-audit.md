# Final XNA 4.0 compatibility audit

Audited 2026-10-04 against CNA `21dc29101` (C ABI 0.44.0) and CNA.NET
`7db0863`. This is the finite compatibility-surface audit requested after the bounded application
sweep. It does not start another application-discovery batch; that batch ended at CSX-151 under
saturation condition B.

## Scope and method

The audit covered the strict `Microsoft.Xna.Framework.*` facade, the opt-in phone facade, the
lower `CNA.Framework` implementation layer, current CNA C headers, compatibility documentation,
and the existing behavioral/differential evidence. It used four independent checks:

1. the metadata verifier against the locally supplied XNA 4.0 reference assemblies;
2. a source scan of every `NotImplementedException`, `NotSupportedException`, `TODO`, and `FIXME`;
3. review of every documented native blocker against the C ABI 0.44.0 headers;
4. the already-completed real-application, XNA IL/oracle, FNA, MonoGame, browser, and Android
   evidence recorded in `CAMPAIGN.md` and the samples repository.

The current metadata measurements are exact:

| Profile | Reference types | CNA.NET types | Unallowlisted diagnostics |
| --- | ---: | ---: | ---: |
| XNA 4.0 Windows runtime | 256 | 256 | 0 |
| XNA 4.0 GamerServices, Avatar and Net | 75 | 75 | 0 |

There are zero executable `throw new NotImplementedException` sites in `src/`. The strict facade
contains no `TODO` or `FIXME`. Its 24 static `NotSupportedException` throw sites were reviewed:
most reproduce XNA behavior (read-only collections, the unreachable `ObjectReader.Read`, invalid
surface identities, and invalid vertex types), while the genuine unsupported cases are listed
below. The two lower-layer TODOs are inherited LZX decoder behavior from FNA and are not missing
public XNA members.

## Finite remaining compatibility backlog

The classification names what remains; it is not a direction to expand the corpus.

| Surface | Classification | Current behavior and reopening condition |
| --- | --- | --- |
| `GraphicsDevice.Present(Rectangle?, Rectangle?, IntPtr)` with any non-default argument | **future work** | The C ABI carries only the device. CNA.NET deliberately throws instead of presenting the wrong region or window. Reopen only with a versioned native presentation descriptor and backend semantics. |
| `GraphicsDevice.ResourceCreated` / `ResourceDestroyed` payloads | **future work** | Handlers can be stored, but no event is fabricated. C ABI 0.44.0 still exposes only created-resource presence and destroyed name/tag presence, not the facade object or round-trippable tag. |
| `VideoPlayer.GetTexture()` stable two-slot identity/lifetime | **platform limitation** | CNA decodes into one borrowed texture, invalidated by the next player call. The generation extension identifies changed pixels but cannot reproduce XNA's two stable frame buffers. |
| `AudioEngine(string, TimeSpan, string)` renderer and look-ahead semantics | **platform limitation** | Values cross the ABI unchanged, but CNA's single audio backend explicitly ignores both. No false renderer-selection or scheduling claim is made. |
| Non-empty `PropertyDictionary` stream values | **future work** | Presence and byte count cross the ABI; bytes do not. Empty values return null and non-empty values refuse deterministically. Reopen only if a stream copy/read route is added. |
| Session leaderboard writer | **intentionally unsupported** | The strict type exists, ordinary/non-Pro Windows XNA use also refuses, and the historical online service is obsolete. CNA C++ has a session writer but the C ABI does not expose it. This is not a reason to revive an extinct service during maintenance. |
| Remaining native-to-XNA exception translations | **future work** | High-value catch boundaries translate canonical C ABI 0.37+ exception names. Other facade calls can still surface `CNA.CnaException` carrying that name. Add a translation only for a reproducible XNA catch contract, not speculatively. |
| GamerServices state failures that do not carry a canonical type | **platform limitation** | The C result alone cannot distinguish `GamerPrivilegeException` or `NetworkNotAvailableException`; the deterministic fallback is `InvalidOperationException`. |
| Local-session packet loopback to the sender's own gamers | **future work** | CNA deliberately does not loop these packets back. XNA's native kernel behavior has not been measured, so no speculative change is justified. |
| Unknown `SpriteSortMode` values | **future work** | This low-value edge remains a timing/type mismatch: XNA stores an unnamed enum; current CNA ultimately refuses it. Non-finite sprite values are fixed by CABI-38 and are no longer part of this row. |
| Windows Forms `Form.Opacity` | **platform limitation** | The compatibility form retains/clamps the value, but the runtime-window C ABI has no opacity operation. MessageBox is implemented by CNA's native dialog route. |
| A blocking `Guide.End*` wait during SIGTERM | **platform limitation** | The managed signal handler cannot return control to the game until the modal operation closes. Normal game exit and signal exit outside that wait are qualified. |
| Authored XACT, Song, and Video success/lifetime breadth | **future work** | More deterministic coverage needs legally redistributable authored fixtures. Existing real applications provide runtime evidence; absence of a reusable fixture is not a stub. |
| Malformed/error XNB comparison against Windows XNA | **future work** | The managed reader has broad error coverage, but the 46-observation Windows runtime snapshot remains an independent oracle task. |
| Phone `Microsoft.Phone.Shell` / `Notification` | **intentionally unsupported** | These are outside the selected XNA Windows runtime profile and were excluded with Yacht by owner decision. `CNA.PhoneCompat` remains opt-in and metadata-unmeasured without legal Phone reference assemblies. |
| Xbox-only APIs and Content Pipeline/build-time assemblies | **future work** | Separate product profiles, not omissions from the audited Windows runtime facade. Reopen only for a deliberate profile campaign. |
| Browser worker-owned canvas optimization | **platform limitation** | Correctness uses Emscripten's proxied WebGL path. Direct canvas transfer is blocked by the current .NET WebAssembly host/thread creation model and is not broad XNA compatibility work. |

## Items that look like gaps but are not

| Item | Classification | Reason |
| --- | --- | --- |
| `TouchCollection` and discovered `NetworkSessionProperties` mutations | **fixed** | Their refusals are the XNA read-only collection contract. Live network-session properties remain writable. |
| `ObjectReader.Read` | **fixed** | XNA's implementation is unreachable and throws; polymorphic objects are dispatched through the serialized reader id. |
| `ResourceContentManager` and `ContentManager.ServiceProvider` | **fixed** | The strict facade supplies both and routes overridden streams through the managed reader. A lower `CNA.Framework` implementation detail is not a strict-facade stub. |
| Unknown content reader or unsupported renderer format/capability | **platform limitation** | These are deterministic malformed-content or backend capability failures, not absent public members. |
| Source-text shader dialect mismatch | **extension/non-XNA** | XNA's strict runtime consumes compiled effects. CNA source effects must use the renderer's reported `ShadingDialect`; robustness against a caller supplying the wrong dialect remains a native extension concern. |
| Readback/cube-face capability identities and deterministic device/input injection hooks | **extension/non-XNA** | These improve diagnostics and testing but are not XNA public behavior. Touch already has deterministic injection; keyboard/mouse/gamepad injection remains optional tooling. |
| `CNA.Framework` and CNB-specific limitations | **extension/non-XNA** | They do not alter the exact `Microsoft.Xna.*` metadata contract. MonoGame/FNA-only extensions are not targets of this campaign. |
| Planned binding generator | **obsolete** | Hand-written strict facades reached exact metadata without it. It is tooling that may be reconsidered only when a concrete repetitive maintenance change justifies it. |

## Native stability follow-ups

These are not missing XNA members, but should remain visible during maintenance:

- repeated game construction on the OPENGL33 renderer has not been requalified on the current
  tree; other qualified renderers complete repeated ownership cycles;
- the graphics-resource device-lifetime token appears to close the historical destruction-time
  virtual call, but the exact old sanitizer reproduction has not been rerun;
- CNA.NET turns SIGTERM/SIGINT into the game's normal exit, avoiding the historical native
  signal-thread teardown path; the pure native abrupt-signal path remains a separate hardening
  issue;
- the C smoke suite has documented high-parallelism timing flakes and must not be reported as a
  deterministic clean pass when one occurs.

## Platform qualification boundary

| Target | Classification | Qualification at this audit |
| --- | --- | --- |
| Linux desktop | **fixed** | Primary qualified platform; broad managed/native suites and unchanged real XNA applications run here. Renderer-specific claims still follow their own tested matrices. |
| WebAssembly | **needs physical/native platform qualification** | Builds and runs in headless Chromium with SwiftShader, including every Microsoft manifest row and representative threaded games. Hardware GPU, interactive-browser behavior, and audio are **not qualified**. |
| Android | **needs physical/native platform qualification** | Builds and runs on the x86_64 emulator, including every Microsoft manifest row. ARM/ARM64, a physical device, audibility, and full physical lifecycle/input behavior are **not qualified**. |
| Windows | **needs physical/native platform qualification** | Build architecture exists; runtime is **not qualified** in this Debian campaign. Wine/XNA oracle runs are evidence for selected programs, not CNA-on-Windows qualification. |
| macOS | **needs physical/native platform qualification** | **Not runtime-qualified.** |
| iOS | **needs physical/native platform qualification** | **Not runtime-qualified.** |

The unqualified targets are separate platform campaigns. They do not keep the bounded CNA.NET
compatibility campaign open.

## Maintenance disposition

The audited backlog is finite. Future CNA.NET compatibility work should begin from a reported
regression, an explicitly requested application, a deliberate subsystem or platform
qualification, or unusually strong differential evidence against real XNA. Do not proactively
search for more games, add FNA/MonoGame extensions for corpus counts, or convert low-value edge
cases into a new broad campaign.
