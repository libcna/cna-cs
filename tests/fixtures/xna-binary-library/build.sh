#!/usr/bin/env bash
# Rebuilds XnaBinaryLibrary.dll as Visual Studio 2010 built an XNA Windows Game Library: for x86,
# against .NET Framework 4.0's and XNA Game Studio 4.0's reference assemblies (XNA_REFERENCE_PATH, as tools/api-compat reads them), with mono's
# C# compiler. The binary is checked in so the tests need neither.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"
xna="${XNA_REFERENCE_PATH:?set XNA_REFERENCE_PATH to the XNA 4.0 reference assemblies}"
api=/usr/lib/mono/4.0-api
mcs -nologo -target:library -platform:x86 -optimize+ -debug- -deterministic -nostdlib \
    -r:"$api/mscorlib.dll" -r:"$api/System.dll" \
    -r:"$xna/Microsoft.Xna.Framework.dll" -r:"$xna/Microsoft.Xna.Framework.Graphics.dll" \
    -r:"$xna/Microsoft.Xna.Framework.Game.dll" \
    -out:XnaBinaryLibrary.dll XnaBinaryLibrary.cs
