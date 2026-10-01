#!/usr/bin/env bash
# Builds CNA's C API for Android and stages what a CNA.NET app packages, under
# build-consumer/cna-native-android/: <abi>/ with libcna_c_api.so, the SDL libraries it needs,
# libc++_shared.so if needed and libmain.so (eng/android/cna_android_main.c); java/ with the SDL Java
# sources of the SDL that was built; PROVENANCE.txt.
#
# Usage: scripts/Build-AndroidNative.sh [--abi x86_64|arm64-v8a] [--ndk DIR] [--cna DIR] [--configure]
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cna_root="$(cd "$here/../cna" && pwd)"
abi=x86_64
ndk="${ANDROID_NDK_ROOT:-$(ls -d "$HOME"/Android/Sdk/ndk/* 2>/dev/null | sort -V | tail -1)}"
configure=0
while [ $# -gt 0 ]; do
    case "$1" in
        --abi)       abi="$2"; shift 2 ;;
        --ndk)       ndk="$2"; shift 2 ;;
        --cna)       cna_root="$(cd "$2" && pwd)"; shift 2 ;;
        --configure) configure=1; shift ;;
        -h|--help)   sed -n '2,8p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
        *)           echo "error: unknown argument $1" >&2; exit 2 ;;
    esac
done
[ -f "$ndk/build/cmake/android.toolchain.cmake" ] || { echo "error: no Android NDK at '$ndk'" >&2; exit 2; }
case "$abi" in
    x86_64)    triple=x86_64-linux-android ;;
    arm64-v8a) triple=aarch64-linux-android ;;
    *)         echo "error: unsupported ABI $abi" >&2; exit 2 ;;
esac
api=24
export CCACHE_DIR=/rv/cnaccache CCACHE_BASEDIR=/rv

build="$cna_root/cmake-build-android-$abi"
if [ "$configure" = 1 ] || [ ! -f "$build/CMakeCache.txt" ]; then
    cmake -S "$cna_root" -B "$build" -G Ninja -DCMAKE_BUILD_TYPE=Release \
        -DCMAKE_TOOLCHAIN_FILE="$ndk/build/cmake/android.toolchain.cmake" \
        -DANDROID_ABI="$abi" -DANDROID_PLATFORM="android-$api" \
        -DCNA_GRAPHICS_RENDERER=OPENGLES3 -DCNA_PLATFORM=SDL3 -DCNA_AUDIO_PLATFORM=SDL3 \
        -DCNA_BUILD_C_API=ON -DCNA_EASYGL_COMPILED_EFFECTS=ON -DCNA_ENABLE_VIDEO=OFF \
        -DCNA_ENABLE_DRACO=OFF -DCNA_CNAEXT=OFF -DCNA_BUILD_TESTS=OFF -DCNA_BUILD_EXAMPLES=OFF \
        -DCNA_C_API_BUILD_STATIC=OFF \
        -DCMAKE_C_COMPILER_LAUNCHER=ccache -DCMAKE_CXX_COMPILER_LAUNCHER=ccache
fi
cmake --build "$build" --target cna_c_api -j8

toolchain="$ndk/toolchains/llvm/prebuilt/linux-x86_64"
readelf="$toolchain/bin/llvm-readelf"
stage="$here/build-consumer/cna-native-android"
mkdir -p "$stage/$abi"
rm -f "$stage/$abi"/*.so

"$toolchain/bin/$triple$api-clang" -shared -fPIC -O2 -Wall -Wextra -Werror \
    -o "$stage/$abi/libmain.so" "$here/eng/android/cna_android_main.c"
cp "$build/modules/c-api/libcna_c_api.so" "$stage/$abi/"

# Stage what libcna_c_api.so asks the loader for, and nothing the system already provides.
sdl_root="$(sed -n 's/^CNA_SDL_PREBUILT_ROOT:PATH=//p' "$build/CMakeCache.txt")"
for needed in $("$readelf" -d "$build/modules/c-api/libcna_c_api.so" | sed -n 's/.*Shared library: \[\(.*\)\].*/\1/p'); do
    if [ -f "$sdl_root/install/lib/$needed" ]; then
        cp -L "$sdl_root/install/lib/$needed" "$stage/$abi/"
    elif [ "$needed" = libc++_shared.so ]; then
        cp "$toolchain/sysroot/usr/lib/$triple/libc++_shared.so" "$stage/$abi/"
    fi
done

# The NDK compiles with -g even for Release: libcna_c_api.so is ~260 MB with its debug info and ~50 MB
# without. An app ships the stripped copies; the build tree keeps the originals for symbolizing.
for library in "$stage/$abi"/*.so; do
    "$toolchain/bin/llvm-strip" --strip-unneeded "$library"
done

# The Java half must be the SDL that was built.
java="$sdl_root/SDL/source/android-project/app/src/main/java/org/libsdl/app"
[ -d "$java" ] || java="$cna_root/third_party/SDL/android-project/app/src/main/java/org/libsdl/app"
rm -rf "$stage/java"
mkdir -p "$stage/java"
cp "$java"/*.java "$stage/java/"

{
    echo "CNA C API for a .NET Android app"
    echo "staged: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
    echo "CNA: $(git -C "$cna_root" rev-parse HEAD)$(git -C "$cna_root" diff --quiet || echo ' (dirty)')"
    echo "NDK: $ndk, API $api"
    echo "SDL Java: $java"
    for library in "$stage/$abi"/*.so; do
        echo "$abi/$(basename "$library"): $(sha256sum "$library" | cut -d' ' -f1)"
    done
} > "$stage/PROVENANCE.txt"
cat "$stage/PROVENANCE.txt"
