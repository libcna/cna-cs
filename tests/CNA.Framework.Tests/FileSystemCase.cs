using System;
using System.IO;
using Xunit;

namespace CNA.Framework.Tests;

/// <summary>
/// Whether the filesystem this run writes to tells two names that differ only in case apart.
///
/// The content-path tests were written on Linux, where it does, and they observe the resolver
/// re-casing a request to the name on disk. On a case-insensitive filesystem -- the default on
/// macOS and on Windows, the platform XNA games were written for -- every casing already names the
/// file, so the resolver's exact-match probe succeeds and it returns the name it was asked for.
/// Both outcomes open the same file; only the string differs (CNA plans/plan_apple_m4.md AM4-218).
/// </summary>
internal static class FileSystemCase
{
    private static readonly Lazy<bool> Sensitive = new(Measure);

    /// <summary>True when both the temporary directory and the test output directory, where these
    /// tests create their files, are case-sensitive.</summary>
    internal static bool IsSensitive => Sensitive.Value;

    /// <summary>
    /// Asserts that <paramref name="resolved"/> names the file created at <paramref name="onDisk"/>:
    /// exactly that name where the filesystem is case-sensitive, and that name in any casing -- an
    /// existing path -- where it is not.
    /// </summary>
    internal static void AssertNamesFile(string onDisk, string resolved, string? existsUnder = null)
    {
        if (IsSensitive)
        {
            Assert.Equal(onDisk, resolved);
            return;
        }
        Assert.Equal(onDisk, resolved, ignoreCase: true);
        string probe = existsUnder is null ? resolved : Path.Combine(existsUnder, resolved);
        Assert.True(File.Exists(probe), $"'{probe}' does not exist");
    }

    private static bool Measure() =>
        DirectoryIsCaseSensitive(Path.GetTempPath()) && DirectoryIsCaseSensitive(AppContext.BaseDirectory);

    private static bool DirectoryIsCaseSensitive(string parent)
    {
        string directory = Path.Combine(parent, "cna-case-probe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllBytes(Path.Combine(directory, "Probe"), []);
            return !File.Exists(Path.Combine(directory, "probe"));
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
        }
    }
}

/// <summary>
/// A <see cref="FactAttribute"/> for a test that can only make its observation where two names
/// differing in case are two files: one that creates both, or one whose evidence is the re-casing
/// itself. Elsewhere it skips and says why, rather than asserting something weaker under the same
/// name.
/// </summary>
public sealed class CaseSensitiveFileSystemFactAttribute : FactAttribute
{
    public CaseSensitiveFileSystemFactAttribute()
    {
        if (!FileSystemCase.IsSensitive)
        {
            Skip = "this filesystem is case-insensitive (the macOS and Windows default): two names that " +
                   "differ only in case are one file, so the observation this test makes cannot occur";
        }
    }
}
