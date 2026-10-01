using System;
using System.IO;
using CNA.Content;
using Xunit;

namespace CNA.Framework.Tests;

/// <summary>
/// File paths a Windows-authored game hands XNA's file-taking APIs resolve as Windows resolved
/// them (cna-cs CSX-096): RolePlayingGame gives <c>AudioEngine</c>
/// <c>Content\Audio\RpgAudio.xgs</c>, ShipGame gives it <c>content/sounds/sounds.xgs</c> for
/// <c>Content/Sounds/sounds.xgs</c>.
/// </summary>
public class XnaContentPathHostTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(), "cna-host-path-" + Guid.NewGuid().ToString("N"));

    public XnaContentPathHostTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
        GC.SuppressFinalize(this);
    }

    private void Write(string relative)
    {
        string path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, [0]);
    }

    private static string Host(params string[] segments) => string.Join(Path.DirectorySeparatorChar, segments);

    [Fact]
    public void BackslashesBecomeTheHostSeparator()
    {
        Write(Host("Content", "Audio", "RpgAudio.xgs"));
        Assert.Equal(Host("Content", "Audio", "RpgAudio.xgs"),
            XnaContentPath.ToHostPath(@"Content\Audio\RpgAudio.xgs", _root));
    }

    [Fact]
    public void EverySegmentMatchesIgnoringCase()
    {
        Write(Host("Content", "Sounds", "sounds.xgs"));
        Assert.Equal(Host("Content", "Sounds", "sounds.xgs"),
            XnaContentPath.ToHostPath("content/sounds/SOUNDS.xgs", _root));
    }

    [Fact]
    public void AnAbsolutePathIsReCasedBelowItsRoot()
    {
        Write(Host("Content", "Audio", "Wave.xwb"));
        Assert.Equal(Path.Combine(_root, "Content", "Audio", "Wave.xwb"),
            XnaContentPath.ToHostPath(Path.Combine(_root, "content", "AUDIO", "wave.xwb")));
    }

    [Fact]
    public void AnExistingPathIsReturnedAsWritten()
    {
        Write(Host("Data", "a.txt"));
        Write(Host("data", "a.txt"));
        Assert.Equal(Host("data", "a.txt"), XnaContentPath.ToHostPath("data/a.txt", _root));
    }

    [Fact]
    public void APathThatMatchesNothingComesBackNormalizedOnly()
    {
        Write(Host("Content", "x.txt"));
        Assert.Equal(Host("Content", "missing", "y.txt"),
            XnaContentPath.ToHostPath(@"Content\missing\y.txt", _root));
    }
}
