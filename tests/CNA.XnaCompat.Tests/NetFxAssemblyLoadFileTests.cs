using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// Assembly.LoadFile as the .NET Framework that XNA 4.0 games target loaded a file: once (CSX-122).
/// This assembly imports CNA.XnaCompat.targets as a game does, so its calls are intercepted. On .NET
/// each LoadFile is a context of its own, and the game's types exist twice.
/// </summary>
public class NetFxAssemblyLoadFileTests
{
    [Fact]
    public void LoadFile_OfAnAssemblyAlreadyLoaded_IsThatAssembly()
    {
        // jacobdufault/forge-sample: LoadFile of its own GameLogic.dll, beside it.
        Assembly referenced = typeof(Microsoft.Xna.Framework.Vector2).Assembly;

        Assembly fromFile = Assembly.LoadFile(referenced.Location);

        Assert.Same(referenced, fromFile);
        Assert.Same(typeof(Microsoft.Xna.Framework.Vector2), fromFile.GetType("Microsoft.Xna.Framework.Vector2"));
    }

    [Fact]
    public void LoadFile_OfAFileTheApplicationWouldLoad_IsTheAssemblyItsReferencesBindTo()
    {
        // A file of the application not loaded yet: its later loads by name must be this assembly.
        string path = TrustedFilesNotLoaded().First();

        Assembly fromFile = Assembly.LoadFile(path);

        Assert.Same(AssemblyLoadContext.Default, AssemblyLoadContext.GetLoadContext(fromFile));
        Assert.Same(fromFile, Assembly.Load(fromFile.GetName()));
        Assert.Same(fromFile, Assembly.LoadFile(path));
    }

    [Fact]
    public void LoadFile_OfACopyElsewhere_IsAnAssemblyOfItsOwn()
    {
        string source = typeof(Microsoft.Xna.Framework.Vector2).Assembly.Location;
        string directory = Directory.CreateTempSubdirectory("cna-loadfile-").FullName;
        string copy = Path.Combine(directory, Path.GetFileName(source));
        File.Copy(source, copy);
        try
        {
            Assembly fromFile = Assembly.LoadFile(copy);

            Assert.NotSame(typeof(Microsoft.Xna.Framework.Vector2).Assembly, fromFile);
            Assert.NotSame(AssemblyLoadContext.Default, AssemblyLoadContext.GetLoadContext(fromFile));
            Assert.Same(fromFile, Assembly.LoadFile(copy));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void LoadFile_RefusesWhatItRefusedBefore()
    {
        Assert.Throws<ArgumentException>(() => Assembly.LoadFile("relative.dll"));
        Assert.Throws<ArgumentNullException>(() => Assembly.LoadFile(null!));
    }

    private static IEnumerable<string> TrustedFilesNotLoaded()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => assembly.Location)
            .ToHashSet(StringComparer.Ordinal);
        return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal))
            .Where(path => !loaded.Contains(path))
            .Order(StringComparer.Ordinal);
    }
}
