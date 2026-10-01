# CNA native ABI compatibility contract

This document is the normative CNA.NET consumer policy for the CNA C ABI. The machine-readable
form is [`eng/cna-native-abi-policy.json`](../eng/cna-native-abi-policy.json); the loader implements
policy `cna-cs-native-abi/1` in `CNA.Interop`. Upstream CNA remains the authority for its C ABI and
is not modified by this policy.

## Version meaning

CNA encodes `major.minor.patch` into a `uint32_t` with 16 bits for major and 8 bits each for minor
and patch. The fields do not imply ordinary stable SemVer while the major is zero:

| Field | CNA C ABI meaning | CNA.NET admission consequence |
| --- | --- | --- |
| Major | At ABI 1.x and later, an incompatible change requires a new major. Major 0 identifies the experimental line and provides no same-major compatibility guarantee. | A different major is rejected. Sharing major 0 is necessary but not sufficient. |
| Minor before 1.0 | A reviewed ABI generation. It may be additive or incompatible; CNA 0.3, 0.6, and 0.8 contain documented contract changes. | Each exact minor must have a reviewed matrix entry. There is no `nativeMinor >= 6` rule. |
| Minor at/after 1.0 | CNA's published contract permits additive, backward-compatible changes within the major. | Future consumers may use a major/minimum-minor rule only after the 1.0 contract and evidence are reviewed into a new CNA.NET policy. |
| Patch | ABI-neutral fixes only: no change to exports, prototypes, layouts, values, callbacks, ownership, errors, or documented behavior. | Exact patch releases still require a matrix entry because today's runtime metadata has a version but no ABI-shape fingerprint. |

The upstream installed CMake package's `SameMajorVersion` selection is not used as runtime proof.
It cannot express CNA's documented experimental-0.x exception by itself.

## Reviewed compatibility matrix

| CNA.NET consumer | Native library | CNA.NET result | Evidence |
| --- | --- | --- | --- |
| 0.35.0 | 0.35.0 | Accept | Consumer baseline at CNA `next` `bdf239360`. |
| 0.35.0 | Any other 0.x | Reject | No audited matrix entry. |
| 0.35.0 | Any 1.x+ | Reject | Different ABI major. |

One accepted entry is not a simplification of the policy; it is what the policy produces when the
consumer moves. There is no `>= 0.35` rule and there never was a `>= 0.6` one.

`bdf239360` matters beyond the version: it carries CBIND-128, without which a drawable component
cannot borrow the game's device from the callbacks the game drives. A 0.35.0 library from before it
is admitted -- the version cannot tell them apart, because CNA treated the fix as a contract
correction within 0.35.0 -- and fails exactly one integration test,
`DrawableComponent_UsesGraphicsDeviceFromGameDrivenCallbacks`, with `InvalidState`.

### Retired entries

0.6.0, 0.7.0, 0.8.0, 0.19.0, 0.20.0 and 0.21.0 were accepted once. 0.6.0-0.8.0 were retired when
this binding began importing routes they do not export; 0.19.0, 0.20.0 and 0.21.0 when the next
reviewed generation superseded them. The retirements are enforced: `retired-0.8.0` and
`retired-0.21.0` are refused.

`retired-0.21.0` is the sharp control. Every route this binding still imports existed in 0.21.0
with the same prototype, so a 0.21.0 library exports every required symbol and passes every runtime
shape probe; the version rule alone refuses it. A policy written as "whatever exports the names we
call" would take it.

### What the 0.35.0 admission measured

0.21.0 (CNA `599d14e54`) to 0.35.0 (CNA `bdf239360`), 2026-10-01:

| Measurement | Result |
| --- | --- |
| Consumed entry points | 957, after dropping 45 (below) |
| Consumed entry points absent / changed prototype | 0 / 0 |
| Upstream findings outside the consumed surface | 1048 removals and changes, reviewed in [`eng/cna-upstream-abi-allowlist.txt`](../eng/cna-upstream-abi-allowlist.txt): 36 entries, 0 stale |
| `tools/abi-verify` | 895 native and 895 managed layout/type values, 0 mismatches; 957 of 957 prototypes compiled; 6 callbacks; 350 constants; 12 of 12 negative controls rejected |
| Documented contracts of the 957 routes, header text diffed | 14 changed |
| Native integration, Debug and Release, OPENGLES3 with compiled effects | 200 of 200 |

**The 45 dropped imports.** CNA removed `engine_layer.h` in 0.30.0 (`MOD-RETIRE-1`): the
post-process chain/pass, bloom, tonemap, render-target pool and blit routes and
`cna_engine_layer_get_version`, 44 routes this binding imported for its `CNA.Graphics.Experimental`
layer (`PostProcessChain`, `RenderTargetPool`, the HDR pass settings). That layer was CNA surface
with no XNA counterpart and was removed rather than rebuilt over a retired native layer;
`RenderTarget2D` and effects remain the XNA way to post-process. `cna_graphics_ext_is_available`
went with it, because its only caller was `GraphicsDevice.IsCnaEngineLayerAvailable`, whose meaning
the retirement removed.

**The 14 changed contracts.** Three needed managed changes:

- `cna_graphics_device_clear_options` now refuses to clear a depth or stencil plane the target does
  not have, as XNA's explicit `Clear` does. `Clear(Color)` had passed all three planes and
  `Viewport.MaxDepth` (FNA's form, which masks missing planes); it is now XNA's own IL,
  `Clear(DefaultClearOptions, color, 1f, 0)`.
- `cna_graphics_device_get_shader_dialect_ext` can answer HLSL, MSL, WGSL and SPIR-V;
  `ShaderDialect` named only the first four identities.
- `cna_graphics_device_supports_capability`, occlusion queries and `cna_effect_set_current_technique`
  now refuse what XNA refuses (Reach-profile limits, query sequencing, a null technique); the
  managed code either already refused first or surfaces the native refusal.

The rest are wording. Four integration tests encoded behaviour XNA does not have and were
corrected rather than the binding: a quad behind the camera that only GL's depth range had drawn,
a separate-alpha blend state in Reach, an occlusion query in Reach, and an audio buffer that was not
a whole number of frames.

## Compatible evolution operations

An operation is compatible only when it preserves every contract an existing consumer can use:

- add an export under a new name without removing or changing an existing export;
- add an unrelated fixed-width constant without renumbering or changing an existing constant;
- append an optional field to a caller-sized/versioned descriptor while retaining the old
  mandatory prefix, accepting the old `struct_size`, and never reading or writing beyond the size
  the caller supplied;
- add a new callback table or append an optional callback to a genuinely caller-sized table while
  honoring the old table size;
- add a fixed-width enum-like identity when existing values and sentinels remain fixed and an old
  consumer cannot be forced to interpret the new value;
- clarify documentation without changing ownership, error, threading, lifetime, or behavior.

Additional exports are deliberately allowed by the loader. They cannot collide with or substitute
for the 957 names imported by this build.

## Breaking operations

The following require a new stable major, or a new reviewed experimental minor plus an explicit
consumer-matrix decision:

- remove, rename, hide, or stop exporting an existing entry point;
- change an export's return type, parameter count/order/type/width/pointer depth, calling
  convention, ownership, errors, threading, lifetime, or behavior;
- change any existing scalar width, constant value, flag bit, handle representation, or string /
  buffer convention;
- reorder, resize, retype, remove, or change the meaning/alignment/packing of an existing struct
  field, or append to a fixed/non-size-aware output struct;
- require a newly appended descriptor field from a caller that supplied the older valid prefix;
- renumber an enum-like identity, change a sentinel such as `MAXIMUM`, or return a new identity to
  a route whose old consumer cannot tolerate unknown values;
- change an existing callback signature, calling convention, invocation order/thread, error
  propagation, context meaning, or lifetime.

Deprecation is compatible only while the old export and its contract remain available. A changed
signature uses a new export name. A changed fixed struct uses a new type and normally a new route.

### Struct-size and version rules

`struct_size`/`struct_version` are useful only when the callee actually reads the caller's header
before accessing the body. Input and in/out descriptors may grow by appending optional fields when
the old prefix remains valid. An output initializer that receives only `T*` and overwrites the
header has no independent capacity argument; CNA.NET therefore treats that output shape as fixed
despite the header. It may grow only through a size-aware/new export or a new type. The loader's
guarded `CNA_TouchCapabilities` initializer canary exists specifically to enforce one foundational
instance of this distinction.

### Enum-like values

CNA exposes fixed-width `uint32_t` typedefs and named constants, not compiler-sized C enums.
Appending a value is compatible only if all old values and sentinels remain stable and old callers
either cannot receive the new value or explicitly tolerate unknown values. Appending a flag bit is
subject to the same rule for masks. Moving `MAXIMUM` is a constant-value change and is breaking for
callers that use it, which is why 0.8 is not labeled generally additive.

### Callbacks

Existing callback prototypes are immutable. New callback capability uses a new subscription or an
optional appended slot in a size-aware table. A callback's thread, ordering, reentrancy, context,
error, and lifetime rules are ABI contract just as much as its machine-level prototype.

## Loader proof and its boundary

Before returning a library handle, the managed resolver now requires:

1. readable `cna_get_abi_version` metadata and an exact reviewed matrix entry;
2. every one of the 957 `LibraryImport` entry points declared by `CNA.Interop.Native`;
3. a successful `cna_error_get_last_message_size` result/out-parameter signature canary;
4. a successful guarded `cna_touch_capabilities_init` canary proving the 16-byte version-1 shape,
   canonical body, and write bounds.

Version numbers and symbol names cannot describe every native prototype or POD layout. The runtime
checks therefore complement, rather than replace, `tools/abi-verify`: the platform C compiler
checks 895 native/managed size, alignment, offset, width and type values, compiles every import's
prototype against the headers, checks the callbacks and asserts the enum-like constants, and rejects
headers outside this same version matrix. CNA's own ABI baseline supplies the reviewed
release-to-release shape/export/value diff, which `tools/coverage/baselinediff.py` computes and which
must report zero unreviewed breaking differences. A new ABI version must pass both evidence paths
before it is added to the matrix.

## Automated fixtures

`scripts/Verify-NativeAbiCompatibility.sh` builds dependency-free shared libraries and runs each in
a fresh managed process. The exact matrix is:

| Fixture | Expected | Property proved |
| --- | --- | --- |
| `exact-0.35.0` | Accept | Exact expected ABI. |
| `exact-0.35.0-extra-symbol` | Accept | Unrelated added exports do not break a consumer. |
| `retired-0.8.0` | Reject | A generation retired because this consumer outgrew it. |
| `retired-0.21.0` | Reject | A generation retired because a newer one superseded it -- being previously audited is not admission. It exports every required symbol and passes every shape probe, so the version rule alone refuses it. |
| `unreviewed-0.34.0` | Reject | Older and unreviewed. |
| `unreviewed-0.36.0` | Reject | Neither is being newer: the matrix is a point list, not a floor. |
| `missing-required-symbol` | Reject | Any missing managed import fails at load, not at first use. |
| `changed-required-signature` | Reject | A testable core signature/out-parameter change fails its canary. |
| `incompatible-major-1.0.0` | Reject | Major mismatch. |
| `structurally-incompatible-0.35.0` | Reject | An accepted version cannot override guarded shape evidence. |
| `malformed-metadata-0.0.0` | Reject | An incomplete/unrecognized encoded generation. |
| `unreadable-metadata` | Reject | Missing version export. |

The fixture inventory is generated from and checked against the managed import declarations. CI
runs this gate without a protected CNA binary; package acceptance reruns it and additionally sends
the selected real library through the same probe.
