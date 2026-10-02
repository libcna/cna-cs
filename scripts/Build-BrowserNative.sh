#!/usr/bin/env bash
# Builds CNA's WebGL2 C API for a browser and stages it where eng/browser/CNA.Browser.targets links
# it from: build-consumer/cna-native-browser-wasm/cna-native.a, with a PROVENANCE.txt.
#
# A .NET browser app links its native code into dotnet.native.wasm with the Emscripten its
# wasm-tools workload pins, so CNA must be compiled with that same Emscripten -- not with whatever
# emsdk is installed. .NET 11's pins 6.0.x; .NET 8 and 10 pin 3.1.x, whose libc++ cannot compile CNA.
#
# Usage: scripts/Build-BrowserNative.sh [--dotnet-root DIR] [--cna DIR] [--configure] [--threads]
#
#   --dotnet-root  the .NET 11 SDK with the wasm-tools workload (default: ~/deps/dotnet11)
#   --cna          the CNA checkout (default: ../cna)
#   --configure    reconfigure cmake-build-webgl2 even if it exists
#   --threads      the shared-memory pthread build (cmake-build-webgl2-threads, staged in
#                  build-consumer/cna-native-browser-wasm-threads) for a WasmEnableThreads game
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
dotnet_root="${DOTNET_ROOT_BROWSER:-$HOME/deps/dotnet11}"
cna_root="$(cd "$here/../cna" && pwd)"
configure=0
threads=0
while [ $# -gt 0 ]; do
    case "$1" in
        --dotnet-root) dotnet_root="$2"; shift 2 ;;
        --cna)         cna_root="$(cd "$2" && pwd)"; shift 2 ;;
        --configure)   configure=1; shift ;;
        --threads)     threads=1; shift ;;
        -h|--help)     sed -n '2,15p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
        *)             echo "error: unknown argument $1" >&2; exit 2 ;;
    esac
done

sdk_pack="$(ls -d "$dotnet_root"/packs/Microsoft.NET.Runtime.Emscripten.*.Sdk.linux-x64/*/tools 2>/dev/null | tail -1)"
cache_pack="$(ls -d "$dotnet_root"/packs/Microsoft.NET.Runtime.Emscripten.*.Cache.linux-x64/*/tools 2>/dev/null | tail -1)"
node="$(ls "$dotnet_root"/packs/Microsoft.NET.Runtime.Emscripten.*.Node.linux-x64/*/tools/bin/node 2>/dev/null | tail -1)"
if [ -z "$sdk_pack" ] || [ -z "$cache_pack" ] || [ -z "$node" ]; then
    echo "error: no Emscripten workload packs under $dotnet_root/packs." >&2
    echo "Install them with: DOTNET_ROOT=$dotnet_root $dotnet_root/dotnet workload install wasm-tools" >&2
    exit 2
fi

export EMSDK_PATH="$sdk_pack"
export DOTNET_EMSCRIPTEN_LLVM_ROOT="$sdk_pack/bin"
export DOTNET_EMSCRIPTEN_BINARYEN_ROOT="$sdk_pack"
export DOTNET_EMSCRIPTEN_NODE_JS="$node"
export EM_CONFIG="$sdk_pack/emscripten/.emscripten"
export EM_CACHE="$cache_pack/emscripten/cache"
export PATH="$sdk_pack/emscripten:$sdk_pack/bin:$(dirname "$node"):$PATH"
export CCACHE_DIR=/rv/cnaccache CCACHE_BASEDIR=/rv

emscripten_version="$(emcc --version | head -1)"
build="$cna_root/cmake-build-webgl2"
stage="$here/build-consumer/cna-native-browser-wasm"
sdl_prebuilt="$cna_root/.sdl-prebuilt-emscripten-dotnet11"
threads_option=OFF
if [ "$threads" = 1 ]; then
    # A shared-memory (pthread) WebAssembly ABI cannot link a single-threaded object, so the threaded
    # CNA -- for a game built with WasmEnableThreads -- is its own variant tree, SDL install and stage.
    build="$cna_root/cmake-build-webgl2-threads"
    stage="$here/build-consumer/cna-native-browser-wasm-threads"
    sdl_prebuilt="$cna_root/.sdl-prebuilt-emscripten-dotnet11-pthreads"
    threads_option=ON
fi
# .NET's browser runtime drives the classic Emscripten FS API, so WasmFS -- which CNA's own threaded
# executables select -- stays off; CNA's storage then keeps its IDBFS persistence.
if [ "$configure" = 1 ] || [ ! -f "$build/CMakeCache.txt" ]; then
    # The SDL install is keyed by the toolchain: SDL objects from another Emscripten do not link.
    emcmake cmake -S "$cna_root" -B "$build" -G Ninja -DCMAKE_BUILD_TYPE=Release \
        -DCNA_GRAPHICS_RENDERER=WEBGL2 -DCNA_PLATFORM=SDL3 -DCNA_AUDIO_PLATFORM=SDL3 \
        -DCNA_BUILD_C_API=ON -DCNA_EASYGL_COMPILED_EFFECTS=ON -DCNA_ENABLE_VIDEO=OFF \
        -DCNA_ENABLE_DRACO=OFF -DCNA_CNAEXT=OFF -DCNA_DEVICES=ON -DCNA_ENABLE_NET=ON \
        -DCNA_BUILD_TESTS=OFF -DCNA_BUILD_EXAMPLES=OFF \
        -DCNA_ENABLE_EMSCRIPTEN_THREADS="$threads_option" -DCNA_EMSCRIPTEN_USE_WASMFS=OFF \
        -DCNA_SDL_PREBUILT_ROOT="$sdl_prebuilt" \
        -DCMAKE_C_COMPILER_LAUNCHER=ccache -DCMAKE_CXX_COMPILER_LAUNCHER=ccache
fi
cmake --build "$build" --target cna_c_api_static -j8

mkdir -p "$stage"
cp "$build/modules/c-api/libcna_c_api_static.a" "$stage/cna-native.a"
# XNA StorageDevice's browser backing store: an IDBFS mount made before main() by CNA's own pre-js
# (modules/storage/src/WebStoragePre.js), which the .NET link adds (eng/browser/CNA.Browser.targets).
cp "$cna_root/modules/storage/src/WebStoragePre.js" "$stage/WebStoragePre.js"
{
    echo "CNA WebGL2 C API for a .NET browser app$( [ "$threads" = 1 ] && echo ", shared-memory pthreads (WasmEnableThreads)")"
    echo "staged: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
    echo "CNA: $(git -C "$cna_root" rev-parse HEAD)$(git -C "$cna_root" diff --quiet || echo ' (dirty)')"
    echo "emscripten: $emscripten_version"
    echo "workload pack: $sdk_pack"
    echo "sha256: $(sha256sum "$stage/cna-native.a" | cut -d' ' -f1)"
} > "$stage/PROVENANCE.txt"
cat "$stage/PROVENANCE.txt"
