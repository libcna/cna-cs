# CNA.NET

> **Status: exact public metadata for the selected XNA 4.0 Windows runtime profile; not yet
> behaviorally complete or release-ready.** The strict comparison reports 256/256 types, zero
> differences, zero CNA leaks, and an empty allowlist. The binding targets CNA C ABI 0.35.0.

CNA.NET is the C#/.NET binding for [CNA](https://github.com/openeggbert/cna), a native C++ game
framework. Its intended path is:

```text
C# XNA-style game
        ↓
CNA.XnaCompat   Microsoft.Xna.Framework public facade
        ↓
CNA.Framework   idiomatic CNA managed implementation
        ↓
CNA.Interop     internal P/Invoke boundary
        ↓
CNA C ABI → CNA C++
```

XNA source compatibility is the priority. CNA's native headers determine available backend
capabilities; authoritative XNA assemblies/documentation determine the managed public contract.
Binary compatibility with Microsoft's strong-named assemblies is not the primary goal.

## Measured state

As of 2026-10-01, against CNA `next` `6e0de68e8` (C ABI 0.35.0), OPENGLES3 with
`CNA_EASYGL_COMPILED_EFFECTS=ON`, Linux x64:

- Debug and Release solution builds: 0 warnings, 0 errors;
- managed tests: 629/629 framework and 269/269 XNA-compat;
- native integration tests: 203/203, plus 7/7 GamerServices integration tests (their own
  process: the native dispatcher is process-wide, as XNA's is);
- strict metadata profile: 256 reference types versus 256 target types, 0 differences, 0
  allowlisted; GamerServices/Avatar/Net profile: 55 of 75 types present and exact, the Avatar
  rendering types and the Net session types still missing;
- ABI: 1383 imports, all resolving; `tools/abi-verify` measures 1119 native/managed layout values
  with 0 mismatches, compiles all 1383 prototypes and asserts 596 constants; the
  `cna-cs-native-abi/1` matrix accepts exactly 0.35.0 and 12 isolated fixtures pass (2 accepted, 10
  rejected). See [`docs/native-abi-compatibility.md`](docs/native-abi-compatibility.md).

The campaign plan and running evidence ledger is [`CAMPAIGN.md`](CAMPAIGN.md). Earlier measurements
are in [`NEXT.md`](NEXT.md) and Git history; treat them as history, not current state.

## Build and test

Requires .NET 8 or later.

```bash
dotnet restore CNA.sln
dotnet build CNA.sln -c Debug --no-restore
dotnet build CNA.sln -c Release --no-restore
dotnet test tests/CNA.Framework.Tests/CNA.Framework.Tests.csproj
dotnet test tests/CNA.XnaCompat.Tests/CNA.XnaCompat.Tests.csproj
```

Native integration tests skip cleanly when CNA is unavailable. To run them explicitly:

```bash
CNA_NATIVE_LIBRARY=/path/to/libcna_c_api.so \
  xvfb-run -a dotnet test tests/CNA.Integration.Tests/CNA.Integration.Tests.csproj
```

The loader also accepts `CNA_NATIVE_DIR`. Explicit configuration is fail-fast and takes precedence
over package-native lookup. Admission follows
[`cna-cs-native-abi/1`](docs/native-abi-compatibility.md), not a same-major range: the version must
have a reviewed matrix entry, all 1383 imports must exist, and signature/shape canaries must pass.
The one accepted entry today is C ABI 0.35.0.
Wrong ABI, missing symbols, conflicts, wrong architecture/load failure, and missing-library cases
report the attempted configuration, consumer/detected ABI where available, RID, and remediation;
`CNA_NATIVE_DIAGNOSTICS=1` enables low-level loader details.

## Strict API verification

Supply legally obtained XNA 4.0 reference assemblies without adding them to this repository:

```bash
dotnet build CNA.sln -c Release
XNA_REFERENCE_PATH=/path/to/xna-reference-assemblies \
  dotnet run --project tools/api-compat -c Release --no-build -- --format text
```

Exit codes are 0 for a clean/reviewed contract, 1 for unallowlisted differences, and 2 for bad
configuration. `--format json` and `--format github` support automation; `--leak-only` requires no
XNA reference assembly. The checked-in allowlist is intentionally empty.

## Repository layout

```text
src/CNA.Interop/                  internal C ABI declarations and marshalling
src/CNA.Framework/                public CNA.* API and native ownership
src/CNA.XnaCompat/                Microsoft.Xna.Framework facade
tests/CNA.Framework.Tests/        managed CNA behavior tests
tests/CNA.XnaCompat.Tests/        managed strict-facade behavior tests
tests/CNA.XnaCompat.CompileProbe/ source-assignability corpus
tests/CNA.XnaCompat.GameCompileProbe/ a real game's source, compiled against the facade
tests/CNA.Integration.Tests/      real native ABI/runtime tests
tools/api-compat/                 signature-aware XNA metadata verifier
tools/abi-verify/                 portable C-authority ABI/layout/prototype verifier
tools/native-abi-probe/           isolated managed native-loader admission probe
tools/behavior-corpus/            authoritative corpus manifest/count/snapshot tooling
tools/content-survey/             how much of a game's compiled content this binding can read
tools/coverage/                   portable header/symbol discovery tools
tools/profile-inventory/          separate future-XNA-profile inventory generator
samples/HelloGame/                small managed sample
```

The sibling `cna-cs-template` is the richer CNA-first demonstration and installable `dotnet new`
template (`cna-game`).

## Architecture and packaging

Public XNA hierarchy takes priority over implementation inheritance. Corrected families use
composition and internal adapters so one native resource has one owner. The selected strict
profile now has no public `CNA.*` base types or public/protected CNA-type signature leaks.

- [`docs/architecture.md`](docs/architecture.md) — layer and ownership rules.
- [`docs/xna-compatibility.md`](docs/xna-compatibility.md) — measured profile and extension boundaries.
- [`docs/packaging.md`](docs/packaging.md) — proposed package/RID graph and measured local acceptance harness.
- [`plan.md`](plan.md) — current measurable roadmap.
- [`NEXT.md`](NEXT.md) — chronological engineering history.

## License

CNA.NET is licensed under the [Microsoft Public License (Ms-PL)](LICENSE). See
[`NOTICE.md`](NOTICE.md) for naming and upstream notices.
