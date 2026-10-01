using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using CNA.Interop;
using Xunit;

namespace CNA.Tests;

/// <summary>
/// <see cref="CnaAbi.Decode"/> is pure arithmetic over the encoding <c>abi.h</c> defines, so it is
/// testable without a native library -- and worth testing, because a wrong shift or mask produces a
/// plausible-looking version rather than an obvious failure, which is exactly the situation the
/// check exists to prevent.
///
/// Native admission runs in isolated fixture processes. These tests pin its pure version matrix
/// and prove that its required-symbol set is the complete managed import surface.
/// </summary>
public class CnaAbiTests
{
    /// <summary>The encoding is <c>(major &amp; 0xFFFF) &lt;&lt; 16 | (minor &amp; 0xFF) &lt;&lt; 8 |
    /// (patch &amp; 0xFF)</c> -- 16 bits for major, 8 each for the rest, which is asymmetric and the
    /// easiest part to get wrong.</summary>
    [Theory]
    [InlineData(0u, 0, 0, 0)]
    [InlineData((1u << 8), 0, 1, 0)]
    [InlineData((1u << 16), 1, 0, 0)]
    [InlineData(1u, 0, 0, 1)]
    [InlineData((2u << 16) | (3u << 8) | 4u, 2, 3, 4)]
    [InlineData((0xFFFFu << 16) | (0xFFu << 8) | 0xFFu, 0xFFFF, 0xFF, 0xFF)]
    public void Decode_SplitsTheFieldsAtTheDocumentedWidths(uint encoded, int major, int minor, int patch)
    {
        Assert.Equal((major, minor, patch), CnaAbi.Decode(encoded));
    }

    /// <summary>
    /// The constant this binding compares against must be the version it was written for, now
    /// 0.36.0, which added the packet-reader copy XNA's managed PacketReader receives through.
    /// Before that it was 0.35.0, reached by dropping the 44 engine-layer imports CNA removed in
    /// 0.30.0 and re-measuring every remaining one, and before that 0.21.0. It sat at 0.6.0 through the generations that only added routes this binding did not
    /// call; it moved to 0.19.0 when the binding started importing routes CNA introduced after
    /// 0.8.0 -- the render-target ContentLost pair, the two optioned raw vertex uploads, the
    /// caller-owned device pair and the engine-layer availability pair -- to 0.20.0 with the
    /// renderer removal, and to 0.21.0 with the device-type query and the object-dictionary
    /// runtime-type-name pair.
    ///
    /// Updating this alongside the constant is the point: a constant that drifts silently would
    /// make the compatibility check pass against a library it should reject.
    /// </summary>
    [Fact]
    public void ExpectedVersion_IsTheAbiThisBindingWasWrittenAgainst()
    {
        Assert.Equal((0, 36, 0), CnaAbi.Decode(CnaAbi.ExpectedVersion));
    }

    /// <summary>Round-trips every field independently, so a mask that swallowed a neighbouring
    /// field's bits would show up rather than cancel out.</summary>
    [Fact]
    public void Decode_FieldsDoNotBleedIntoEachOther()
    {
        Assert.Equal((0, 0, 0xFF), CnaAbi.Decode(0xFFu));
        Assert.Equal((0, 0xFF, 0), CnaAbi.Decode(0xFF00u));
        Assert.Equal((0xFFFF, 0, 0), CnaAbi.Decode(0xFFFF0000u));
    }

    [Theory]
    [InlineData(0, 36, 0, "exact")]
    public void Policy_AcceptsOnlyReviewedAbiGenerations(int major, int minor, int patch, string classification)
    {
        uint version = ((uint)major << 16) | ((uint)minor << 8) | (uint)patch;
        Assert.True(CnaNativeAbiPolicy.TryGetProfile(version, out CnaNativeAbiProfile profile));
        Assert.Equal(classification, profile.Compatibility);
    }

    /// <summary>
    /// 0.6.0 through 0.35.0 are here rather than in the accepting theory above because they were
    /// retired, not because they were never reviewed. 0.35.0 and 0.37.0 sit on either side of the
    /// accepted entry to keep the matrix a point list -- being newer than an audited generation is
    /// not evidence about an experimental 0.x ABI, and neither is having been audited once.
    ///
    /// 0.21.0 is the sharpest of them for removed routes: every route this binding still imports
    /// existed there with the same prototype except 0.36.0's packet-reader copy, so the version
    /// rule is what refuses it. A policy expressed as "whatever exports what we call" would admit
    /// a 0.35.0 library that happened to carry that one route.
    /// </summary>
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 6, 0)]
    [InlineData(0, 7, 0)]
    [InlineData(0, 8, 0)]
    [InlineData(0, 19, 0)]
    [InlineData(0, 20, 0)]
    [InlineData(0, 21, 0)]
    [InlineData(0, 34, 0)]
    [InlineData(0, 35, 0)]
    [InlineData(0, 36, 1)]
    [InlineData(0, 37, 0)]
    [InlineData(1, 0, 0)]
    public void Policy_RejectsUnauditedVersions(int major, int minor, int patch)
    {
        uint version = ((uint)major << 16) | ((uint)minor << 8) | (uint)patch;
        Assert.False(CnaNativeAbiPolicy.TryGetProfile(version, out _));
    }

    [Fact]
    public void RequiredSymbols_AreEveryDeclaredNativeImport()
    {
        string[] declared = typeof(Native)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Select(method => (Method: method, Import: method.GetCustomAttribute<LibraryImportAttribute>()))
            .Where(item => string.Equals(item.Import?.LibraryName, "cna-native", StringComparison.Ordinal))
            .Select(item => string.IsNullOrWhiteSpace(item.Import!.EntryPoint)
                ? item.Method.Name
                : item.Import.EntryPoint!)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        // 1384: the 1002 the 0.21.0 binding imported, less the 44 engine-layer routes CNA retired in
        // 0.30.0 and cna_graphics_ext_is_available, whose only caller described that layer (957),
        // plus the 239 routes of gamer_services.h and the 187 of net.h, net_gamers.h and
        // net_sessions.h that the XNA GamerServices/Avatar/Net surface is built on, plus 0.36.0's
        // cna_packet_reader_copy_data_ext, which XNA's managed PacketReader receives through.
        // The literal is a tripwire, not a fact about CNA -- it exists so that adding an import is
        // a deliberate act rather than something that happens on the way to something else.
        Assert.Equal(1384, declared.Length);
        Assert.Equal(declared, CnaNativeAbiPolicy.RequiredSymbols);
    }

    [Fact]
    public void MachineReadablePolicy_MatchesExecutableMatrix()
    {
        using JsonDocument document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "cna-native-abi-policy.json")));
        JsonElement root = document.RootElement;

        Assert.Equal(CnaNativeAbiPolicy.PolicyVersion, root.GetProperty("policyVersion").GetString());
        Assert.Equal("0.36.0", root.GetProperty("consumerAbi").GetString());
        JsonElement[] entries = root.GetProperty("acceptedVersions").EnumerateArray().ToArray();
        string[] versions = entries.Select(item => item.GetProperty("libraryAbi").GetString()!).ToArray();
        Assert.Equal(
            CnaNativeAbiPolicy.AcceptedProfiles.Select(profile => Format(profile.Version)),
            versions);
        Assert.Equal(
            CnaNativeAbiPolicy.AcceptedProfiles.Select(profile => profile.Compatibility),
            entries.Select(item => item.GetProperty("classification").GetString()));
        Assert.Equal(12, root.GetProperty("fixtures").GetArrayLength());
    }

    private static string Format(uint version)
    {
        (int major, int minor, int patch) = CnaAbi.Decode(version);
        return $"{major}.{minor}.{patch}";
    }
}
