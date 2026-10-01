using System.Reflection;
using GraphicsProfile = Microsoft.Xna.Framework.Graphics.GraphicsProfile;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// XNA chooses a game's default GraphicsProfile from the "Microsoft.Xna.Framework.RuntimeProfile"
/// resource its build embedded from the project's XnaProfile. This test project declares
/// <c>&lt;XnaProfile&gt;HiDef&lt;/XnaProfile&gt;</c> and imports build/CNA.XnaCompat.targets, as a
/// ported game does, so both halves are exercised: the build writes the resource, and
/// GraphicsDeviceManager's reader -- the one its constructor calls -- finds HiDef in it.
/// </summary>
public class RuntimeProfileTests
{
    private static GraphicsProfile Read(Assembly assembly) =>
        (GraphicsProfile)typeof(global::Microsoft.Xna.Framework.GraphicsDeviceManager)
            .GetMethod("ReadDefaultGraphicsProfile", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [assembly])!;

    [Fact]
    public void ProjectXnaProfile_IsEmbeddedAsXnaWritesIt()
    {
        using Stream? stream = typeof(RuntimeProfileTests).Assembly
            .GetManifestResourceStream("Microsoft.Xna.Framework.RuntimeProfile");
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream!);
        Assert.Equal("Windows.v4.0.HiDef", reader.ReadLine());
    }

    [Fact]
    public void DefaultProfile_ComesFromTheGameAssembly()
    {
        Assert.Equal(GraphicsProfile.HiDef, Read(typeof(RuntimeProfileTests).Assembly));
    }

    [Fact]
    public void DefaultProfile_IsReachWithoutTheResource()
    {
        Assert.Equal(GraphicsProfile.Reach, Read(typeof(object).Assembly));
    }

    private static bool KeepsFullScreenInTheGame(string? line, bool hostIsPhone) =>
        (bool)typeof(global::Microsoft.Xna.Framework.GraphicsDeviceManager)
            .GetMethod("KeepsFullScreenInTheGame", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [line, hostIsPhone])!;

    /// <summary>cna-cs CSX-094: a Windows Phone title's full screen is the phone's status bar,
    /// never a display mode, except where the host is a phone.</summary>
    [Theory]
    [InlineData("WindowsPhone.v4.0.Reach", false, true)]
    [InlineData("WindowsPhone.v4.0.Reach", true, false)]
    [InlineData("Windows.v4.0.HiDef", false, false)]
    [InlineData("Xbox360.v4.0.HiDef", false, false)]
    [InlineData(null, false, false)]
    public void PhoneTitle_KeepsFullScreenInTheGame_OnlyOffAPhone(string? line, bool hostIsPhone, bool expected)
    {
        Assert.Equal(expected, KeepsFullScreenInTheGame(line, hostIsPhone));
    }
}
