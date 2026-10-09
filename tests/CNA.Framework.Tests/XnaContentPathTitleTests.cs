using System;
using System.IO;
using CNA.Content;
using Xunit;

namespace CNA.Framework.Tests;

[CollectionDefinition(nameof(WorkingDirectoryCollection), DisableParallelization = true)]
public sealed class WorkingDirectoryCollection;

/// <summary>
/// A relative content root is the title's. XNA's <c>ContentManager.OpenStream</c> reads a relative
/// content path through <c>TitleContainer</c>, which opens it under <c>TitleLocation.Path</c> (XNA
/// IL); CNA.NET probed it under the working directory, so a game started from any other directory
/// -- or on Android, where the working directory is <c>/</c> -- found none of its managed-loaded
/// content ("Content file 'tank' was not found"). These move the working directory away from the
/// title for their duration, so they run alone.
/// </summary>
[Collection(nameof(WorkingDirectoryCollection))]
public sealed class XnaContentPathTitleTests : IDisposable
{
    private readonly string _rootName = "cna-title-root-" + Guid.NewGuid().ToString("N");
    private readonly string _titleRoot;
    private readonly string _elsewhere = Path.Combine(
        Path.GetTempPath(), "cna-working-directory-" + Guid.NewGuid().ToString("N"));
    private readonly string _previousWorkingDirectory = Directory.GetCurrentDirectory();

    public XnaContentPathTitleTests()
    {
        _titleRoot = Path.Combine(AppContext.BaseDirectory, _rootName);
        Directory.CreateDirectory(_titleRoot);
        File.WriteAllBytes(Path.Combine(_titleRoot, "Map1.xnb"), [0]);
        Directory.CreateDirectory(_elsewhere);
        Directory.SetCurrentDirectory(_elsewhere);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_previousWorkingDirectory);
        try { Directory.Delete(_titleRoot, recursive: true); } catch (IOException) { }
        try { Directory.Delete(_elsewhere, recursive: true); } catch (IOException) { }
    }

    [CaseSensitiveFileSystemFact]
    public void ARelativeRootIsProbedUnderTheTitleNotTheWorkingDirectory()
    {
        // The re-cased name proves the probe found the file: it only exists under the title.
        Assert.Equal(Path.Combine(_rootName, "Map1.xnb"), XnaContentPath.ToFilePath(_rootName, "map1", ".xnb"));
    }

    [Fact]
    public void ATitleFilePathOpensWhereverTheGameWasStartedFrom()
    {
        string path = XnaContentPath.ToTitleFilePath(_rootName, "map1", ".xnb");

        FileSystemCase.AssertNamesFile(Path.Combine(_titleRoot, "Map1.xnb"), path);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void AnAbsoluteRootIsNotMovedUnderTheTitle()
    {
        Assert.Equal(Path.Combine(_elsewhere, "rock.xnb"), XnaContentPath.ToTitleFilePath(_elsewhere, "rock", ".xnb"));
    }
}
