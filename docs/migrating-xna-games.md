# Build and migrate XNA C# games with CNA.NET

This guide explains how to create a C# game for CNA or recompile an existing XNA 4.0, FNA, or
MonoGame game against CNA.NET. It covers Linux and Windows desktop builds, WebAssembly, Android,
Visual Studio, and Visual Studio Code.

## Current release status

CNA and CNA.NET are beta, source-first projects. There are currently no official downloadable
CNA native binaries or published CNA.NET packages. Build both repositories from source and point
the game at those builds. A dedicated stabilization phase is currently expected to begin in
January 2027; that is a roadmap expectation, not a binary-release date or support guarantee.

CNA.NET's strict compatibility target is Microsoft XNA 4.0. Source compatibility is the primary
goal. FNA and MonoGame projects migrate cleanly when they stay on the XNA 4.0 API, but CNA.NET does
not promise their engine-specific extensions.

The strongest current runtime qualification is Linux x86_64. Browser execution is qualified in
headless Chromium with SwiftShader, and Android in an x86_64 emulator. Windows, interactive browser
hardware GPU/audio, physical Android/ARM, macOS, and iOS runtime qualification are separate future
platform campaigns. A successful build alone is not a runtime-support claim.

## Choose a starting point

For a new game, use `cna-dotnet-template`. It is an installable `dotnet new` C# project with one shared
game source set and optional browser and Android heads.

For an existing XNA game, keep the original `.cs` files and create a small SDK-style wrapper
project. The wrapper selects the original source files, references CNA.NET, and copies the original
XNA-built content. This keeps application logic unchanged and makes the migration easy to review.

For a complex port, use the projects under `cna-dotnet-samples/games/` as working examples. They cover
custom content readers, compiled effects, XACT, prebuilt XNA libraries, Windows Phone projects,
threading, storage, GamerServices, and old project layouts.

## Source checkout layout

CNA's source build expects its native dependencies as sibling repositories. A convenient layout is:

```text
workspace/
  cna/
  cna-dotnet/
  cna-dotnet-template/
  sharp-runtime/
  easy-gl/
  meta-gl/
```

Initialize CNA's vendored SDL repositories before the first native build:

```bash
cd /path/to/workspace/cna
git submodule update --init --recursive
```

Desktop development requires a C++23 compiler, CMake 3.20 or newer, and .NET 8 or newer. Browser
and Android heads currently require .NET 11 and the corresponding workload. CNA's main README has
the full native dependency list, including optional FFmpeg video support.

## Build CNA and CNA.NET for desktop

### Linux

The qualified Linux configuration uses the OPENGLES3 renderer and the C ABI shared library:

```bash
export CNA_ROOT=/path/to/workspace/cna
export CNA_DOTNET_ROOT=/path/to/workspace/cna-dotnet

cmake -S "$CNA_ROOT" -B "$CNA_ROOT/build" -G Ninja \
  -DCMAKE_BUILD_TYPE=Release \
  -DCNA_GRAPHICS_RENDERER=OPENGLES3 \
  -DCNA_BUILD_C_API=ON \
  -DCNA_EASYGL_COMPILED_EFFECTS=ON
cmake --build "$CNA_ROOT/build" --target cna_c_api --parallel

dotnet build "$CNA_DOTNET_ROOT/CNA.sln" -c Release
```

The native library is normally:

```text
cna/build/modules/c-api/libcna_c_api.so
```

Configure the managed loader with its absolute path:

```bash
export CNA_NATIVE_LIBRARY="$CNA_ROOT/build/modules/c-api/libcna_c_api.so"
```

`CNA_NATIVE_DIR` may name the containing directory instead. An explicit `CNA_NATIVE_LIBRARY` is
fail-fast and takes precedence.

### Windows

Use Visual Studio 2022 with Desktop development with C++, CMake, and the .NET SDK. SDL_RENDERER is
the conservative desktop choice for a first source build:

```powershell
$CnaRoot = "C:\src\cna"
$CnaDotnetRoot = "C:\src\cna-dotnet"

cmake -S $CnaRoot -B "$CnaRoot\build" -G "Visual Studio 17 2022" -A x64 `
  -DCNA_GRAPHICS_RENDERER=SDL_RENDERER `
  -DCNA_BUILD_C_API=ON
cmake --build "$CnaRoot\build" --config Release --target cna_c_api

dotnet build "$CnaDotnetRoot\CNA.sln" -c Release
```

Find the generated `cna_c_api.dll` under the CMake build tree and set its absolute path before
starting the game:

```powershell
$env:CNA_NATIVE_LIBRARY = "C:\src\cna\build\modules\c-api\Release\cna_c_api.dll"
```

The exact multi-configuration output directory can vary with the CMake generator. Native Windows
runtime behavior has not yet received the Linux campaign's full qualification, so treat this as a
source-build workflow rather than a final platform-support promise.

## Create a game from the CNA C# template

Install the local template and generate a development consumer:

```bash
dotnet new install /path/to/workspace/cna-dotnet-template
dotnet new cna-game --name MyGame
cd MyGame

CNA_DOTNET_ROOT=/path/to/workspace/cna-dotnet dotnet build
CNA_DOTNET_ROOT=/path/to/workspace/cna-dotnet \
CNA_NATIVE_LIBRARY=/path/to/workspace/cna/build/modules/c-api/libcna_c_api.so \
dotnet run
```

On PowerShell:

```powershell
$env:CNA_DOTNET_ROOT = "C:\src\cna-dotnet"
$env:CNA_NATIVE_LIBRARY = "C:\src\cna\build\modules\c-api\Release\cna_c_api.dll"
dotnet build
dotnet run
```

The generated project uses the XNA namespaces supplied by `CNA.XnaCompat`. Its `Platforms/Browser`
and `Platforms/Android` projects compile the same C# source and content for those heads.

Package consumer mode also exists for CNA.NET's local package-acceptance tests, but there is no
public package feed yet. Ordinary users should use development/source mode until packages and
RID-native binaries are published.

## Migrate an XNA 4.0 game without changing its C# source

### 1. Preserve the original project as evidence

Keep the original XNA `.csproj`, content project, source tree, and built output. Record which XNA
configuration you are migrating:

- Windows or Windows Phone;
- Reach or HiDef;
- Debug or Release;
- original conditional compilation symbols;
- exact source files selected by the old project.

Do not blindly glob every `.cs` file in an old repository. Many projects contain unused prototypes,
platform alternatives, or generated files that their XNA project did not compile.

### 2. Add an SDK-style wrapper project

This minimal development-mode wrapper illustrates the important pieces:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>OriginalGame</AssemblyName>
    <RootNamespace>OriginalGame</RootNamespace>
    <StartupObject>OriginalGame.Program</StartupObject>
    <XnaProfile>HiDef</XnaProfile>
    <XnaPlatform>Windows</XnaPlatform>
    <DefineConstants>$(DefineConstants);WINDOWS</DefineConstants>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <CnaDotnetRoot Condition="'$(CnaDotnetRoot)' == '' and '$(CNA_DOTNET_ROOT)' != ''">$(CNA_DOTNET_ROOT)</CnaDotnetRoot>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="../OriginalGame/Game1.cs" />
    <Compile Include="../OriginalGame/Program.cs" />
    <Compile Include="../OriginalGame/Properties/AssemblyInfo.cs" />
    <ProjectReference Include="$(CnaDotnetRoot)/src/CNA.XnaCompat/CNA.XnaCompat.csproj" />
    <None Include="../OriginalGame/bin/x86/Release/Content/**/*"
          LinkBase="Content"
          CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <Import Project="$(CnaDotnetRoot)/src/CNA.XnaCompat/build/CNA.XnaCompat.targets"
          Condition="'$(CnaDotnetRoot)' != ''" />

  <Target Name="ValidateCnaReference" BeforeTargets="ResolveProjectReferences">
    <Error Condition="'$(CnaDotnetRoot)' == ''"
           Text="Set CNA_DOTNET_ROOT or pass -p:CnaDotnetRoot=/path/to/cna-dotnet." />
  </Target>
</Project>
```

List the original project's actual `Compile` items for the most faithful wrapper. If you use a glob,
exclude `bin`, `obj`, platform alternatives, and generated files explicitly. Remove
`GenerateAssemblyInfo=false` when the old project did not compile its own `AssemblyInfo.cs`.

`XnaProfile` is important: CNA.NET embeds the same runtime-profile resource that XNA's build used,
so `GraphicsDeviceManager` chooses Reach or HiDef correctly. Preserve the original platform symbols
such as `WINDOWS` or `WINDOWS_PHONE`; do not add MonoGame's or Android's implicit symbols to the
shared game source merely because the new host runs there.

### 3. Keep authentic XNA content

Prefer the game's original XNA 4.0 `.xnb`, `.xgs`, `.xwb`, and `.xsb` files. If the repository does
not ship compiled content, build its original `.contentproj` with XNA 4.0's BuildContent tooling.
The `cna-dotnet-samples/scripts/build-xna-content.sh` workflow is a maintained example for Linux/Wine.

Keep content at the same path below `Content/` and preserve `Content.RootDirectory`. Watch for:

- Windows case-insensitive paths when running on a case-sensitive file system;
- backslashes used through ordinary `System.IO` instead of XNA APIs;
- custom content readers whose runtime assemblies must be referenced and copied;
- compiled effects and the original Reach/HiDef profile;
- XNA songs whose `.xnb` names WMA data. CNA can use an `.ogg`, `.oga`, or `.qoa` conversion beside
  the unchanged song `.xnb`; record any conversion or content substitution.

Do not replace content just to get a green build. A missing commercial font, proprietary asset, or
third-party pipeline extension is an external migration limitation and should be documented as such.

### 4. Handle libraries compiled against XNA

Source libraries should normally be rebuilt against `CNA.XnaCompat`. A prebuilt pure-IL library
that references XNA 4.0 can use CNA.NET's XNA-named forwarding assemblies. Add only the assemblies
the library actually references, for example:

```xml
<ItemGroup>
  <Reference Include="GameLibrary" HintPath="../OriginalGame/GameLibrary.dll" />
  <ProjectReference Include="$(CnaDotnetRoot)/src/XnaAssemblies/Microsoft.Xna.Framework/Microsoft.Xna.Framework.csproj" />
  <ProjectReference Include="$(CnaDotnetRoot)/src/XnaAssemblies/Microsoft.Xna.Framework.Game/Microsoft.Xna.Framework.Game.csproj" />
  <ProjectReference Include="$(CnaDotnetRoot)/src/XnaAssemblies/Microsoft.Xna.Framework.Graphics/Microsoft.Xna.Framework.Graphics.csproj" />
</ItemGroup>
```

Native, mixed-mode, x86-only, or Windows-specific libraries still need their own compatible build.
CNA.NET cannot make an arbitrary native dependency portable.

### 5. Build and run

```bash
CNA_DOTNET_ROOT=/path/to/cna-dotnet dotnet build OriginalGame.CNA.csproj -c Release
CNA_NATIVE_LIBRARY=/absolute/path/to/libcna_c_api.so \
  dotnet run --project OriginalGame.CNA.csproj -c Release
```

Run from the working directory the original game expected. XACT projects in particular often open
banks relative to the process working directory.

## Migrate from FNA

FNA deliberately stays close to XNA, so an FNA game that uses only XNA 4.0 APIs usually needs only
project changes:

1. remove the `FNA.dll` reference;
2. add the `CNA.XnaCompat` project reference and targets import shown above;
3. retain the `Microsoft.Xna.Framework.*` namespaces and game source;
4. point the game at its original XNA/FNA-compatible content;
5. build and run against `cna_c_api`.

Audit every FNA extension (`*EXT`, FNA-specific environment variables, raw FNA3D access, custom
audio/video behavior). Do not replace one extension with a superficially similar CNA feature
without checking semantics. Either isolate it behind an engine adapter, implement a general
XNA-valued compatibility fix, or record it as outside the strict XNA target.

## Migrate from MonoGame

MonoGame source also keeps XNA's namespaces. For a project that uses the XNA 4.0 subset:

1. remove `MonoGame.Framework.*` package references and MonoGame platform bootstrap code;
2. add `CNA.XnaCompat` and use the ordinary XNA `Program.Main` plus `Game.Run` lifecycle;
3. preserve the original XNA profile and compilation symbols;
4. use XNA-built content where available rather than assuming every MGCB extension is an XNA
   content type;
5. audit MonoGame-only APIs, shader dialects, content processors, window hooks, and platform
   services separately.

CNA.NET does not implement MonoGame extensions merely to increase the number of projects that
compile. If a project is substantially a MonoGame application rather than an XNA 4.0 application,
port the engine-specific layer deliberately or keep MonoGame as that project's backend.

The CNA template can compile the same small source set against CNA, FNA, or MonoGame and is useful
as an adapter example:

```bash
dotnet build -p:Engine=MonoGame
FNA_FRAMEWORK_PATH=/path/to/FNA.dll dotnet build -p:Engine=FNA
CNA_DOTNET_ROOT=/path/to/cna-dotnet dotnet build -p:Engine=CNA
```

## Visual Studio

Install the template from a Developer PowerShell and generate a project:

```powershell
dotnet new install C:\src\cna-dotnet-template
dotnet new cna-game --name MyGame
```

Open `MyGame.csproj` or its containing folder in Visual Studio. The template is tagged as a C#
project so Visual Studio versions that surface locally installed `dotnet new` templates can also
show it in the New Project dialog. The CLI path remains authoritative.

Set `CNA_DOTNET_ROOT` and `CNA_NATIVE_LIBRARY` in the shell that starts Visual Studio, or configure them
as debug environment variables for the project. Build the native `cna_c_api.dll` first; Visual
Studio building the C# project does not build CNA automatically.

For an existing game, keep the original XNA solution read-only and add the SDK-style CNA wrapper as
a new project or solution. This prevents Visual Studio's upgrade tools from rewriting the original
XNA project or source list.

## Visual Studio Code

Open the generated game or wrapper folder and use the integrated terminal:

```bash
code MyGame
CNA_DOTNET_ROOT=/path/to/cna-dotnet dotnet build
CNA_DOTNET_ROOT=/path/to/cna-dotnet \
CNA_NATIVE_LIBRARY=/absolute/path/to/libcna_c_api.so \
dotnet run
```

The C# extension or C# Dev Kit provides editing and debugging. A local `.vscode/launch.json` can
pass the native library to the debugger without committing a machine-specific absolute path:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "CNA game",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/bin/Debug/net8.0/MyGame.dll",
      "cwd": "${workspaceFolder}",
      "env": {
        "CNA_DOTNET_ROOT": "/path/to/cna-dotnet",
        "CNA_NATIVE_LIBRARY": "/absolute/path/to/libcna_c_api.so"
      }
    }
  ]
}
```

Adjust the assembly name, target framework, and native-library extension for the host platform.

## Run the same game in a browser

The maintained browser build workflow currently runs on Linux. On a Windows workstation, use a
Linux environment such as WSL 2 for this source-build path.

Install a .NET 11 SDK and its WebAssembly workload, then stage CNA's static WebGL2 archive:

```bash
export DOTNET_ROOT_BROWSER=/path/to/dotnet11
"$DOTNET_ROOT_BROWSER/dotnet" workload install wasm-tools

cd /path/to/cna-dotnet
./scripts/Build-BrowserNative.sh --dotnet-root "$DOTNET_ROOT_BROWSER" --configure
```

`--configure` is needed for the first build or a genuine configuration change, not every rebuild.
Publish the template's browser head with the same shared source:

```bash
cd /path/to/MyGame
CNA_DOTNET_ROOT=/path/to/cna-dotnet \
  "$DOTNET_ROOT_BROWSER/dotnet" publish Platforms/Browser -c Release

cd Platforms/Browser/bin/Release/net11.0/publish/wwwroot
python3 -m http.server 8080
```

Open `http://localhost:8080/`. Do not open `index.html` as a `file:` URL.

A game that creates managed threads needs the shared-memory native build:

```bash
cd /path/to/cna-dotnet
./scripts/Build-BrowserNative.sh --dotnet-root "$DOTNET_ROOT_BROWSER" --threads --configure
```

Set `<WasmEnableThreads>true</WasmEnableThreads>` in the browser head and serve it with COOP and
COEP headers so the page is cross-origin isolated. Threaded WebGL calls currently proxy to the
page thread and can be slow. Browser video, UDP networking, hardware-GPU behavior, interactive
browser behavior, and audio are not generally qualified.

## Run the same game on Android

The maintained Android native staging script currently runs on Linux and needs the Android SDK,
NDK, and a .NET 11 SDK with the Android workload:

```bash
export DOTNET_ROOT_ANDROID=/path/to/dotnet11
export ANDROID_SDK_ROOT=/path/to/Android/Sdk
export ANDROID_NDK_ROOT="$ANDROID_SDK_ROOT/ndk/your-ndk-version"

"$DOTNET_ROOT_ANDROID/dotnet" workload install android

cd /path/to/cna-dotnet
./scripts/Build-AndroidNative.sh --abi x86_64 --ndk "$ANDROID_NDK_ROOT" --configure
```

With an x86_64 emulator running, build and install the template head:

```bash
cd /path/to/MyGame
CNA_DOTNET_ROOT=/path/to/cna-dotnet \
  "$DOTNET_ROOT_ANDROID/dotnet" build Platforms/Android -c Release -t:Install
```

For an ARM64 package, stage `arm64-v8a` and select the matching .NET runtime identifier:

```bash
cd /path/to/cna-dotnet
./scripts/Build-AndroidNative.sh --abi arm64-v8a --ndk "$ANDROID_NDK_ROOT" --configure

cd /path/to/MyGame
CNA_DOTNET_ROOT=/path/to/cna-dotnet \
  "$DOTNET_ROOT_ANDROID/dotnet" build Platforms/Android -c Release \
  -p:RuntimeIdentifier=android-arm64
```

The Android head packages `Content/` as assets and extracts it beside the game before `Main` runs.
Only the x86_64 emulator has broad campaign evidence today. ARM64 packaging is implemented, but a
physical device, lifecycle/input breadth, and audible audio still require formal qualification.

## Optional compatibility components

- `CNA.WindowsFormsCompat` is a deliberately small XNA-valued subset: game-window lookup,
  border style, and native message boxes. It is not a general Windows Forms implementation.
  `Opacity` is stored but cannot be applied through the current native window API.
- `CNA.PhoneCompat` provides the Windows Phone device and lifecycle surface used by the qualified
  XNA samples. It is not a Silverlight/XAML phone host.
- XNA-era `BinaryFormatter` support is enabled for compatible application builds, including the
  modern out-of-band runtime package where necessary. It exists only for trusted legacy game data.
  Never deserialize untrusted data; set `<CnaBinaryFormatter>false</CnaBinaryFormatter>` when the
  game does not need it.

## Troubleshooting

### The native library cannot be found

Use an absolute `CNA_NATIVE_LIBRARY` path. Check that the file matches the process architecture and
C ABI version. CNA.NET accepts only explicitly audited ABI versions; it does not accept an unknown
newer library merely because the major version matches.

### The game compiles but content does not load

Verify the working directory, `Content.RootDirectory`, filename case, and exact output path. Confirm
that the `.xnb` was built for XNA 4.0 and that any custom reader assembly is present. Do not diagnose
content failures by replacing the asset before checking the original pipeline output.

### The original project is x86

Most managed XNA source can be rebuilt for x64/AnyCPU. A prebuilt library may still contain native
code or an explicit 32-bit layout that cannot run in a 64-bit process. Pure-IL XNA libraries can use
CNA.NET's forwarding and layout compatibility; native/mixed-mode dependencies need a compatible
build.

### The game uses Windows-only APIs

Separate XNA compatibility from host compatibility. Win32 P/Invoke, WPF, full Windows Forms,
Silverlight, dead network services, missing commercial fonts, and proprietary middleware are not
XNA APIs and are not automatically supplied by CNA.NET.

### The browser build says no native archive exists

Run `cna-dotnet/scripts/Build-BrowserNative.sh` with the same .NET SDK/workload used to publish the
application. A static archive built by an unrelated Emscripten version is not a safe substitute.

### The Android build says no staged libraries exist

Run `cna-dotnet/scripts/Build-AndroidNative.sh` for every ABI the package targets. Keep the Android
project's `RuntimeIdentifier` aligned with the staged ABI.

## What to report with a compatibility problem

Include the CNA and CNA.NET commit IDs, C ABI version, target OS/architecture, renderer, complete
exception and native log, original project/profile, content provenance, and the smallest unchanged
source reproduction. State whether the same code and content run on real XNA, FNA, or MonoGame.
That evidence makes it possible to fix the responsible general layer and add a regression test.

