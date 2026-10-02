using System.IO.IsolatedStorage;
using Xunit;

namespace CNA.BrowserCompat.Tests;

/// <summary>
/// The browser's isolated storage behaves as .NET's does on the desktop, where CNA.NET games use
/// .NET's own: each expectation below was measured there first (an unlimited quota and no used size,
/// a silent delete of a missing file, IO failures wrapped in IsolatedStorageException with the IO
/// exception inside, a closed store refused) -- except that a backslash separates directories, as
/// it did on Windows Phone, where these games saved.
/// </summary>
public sealed class IsolatedStorageFileTests : IDisposable
{
    private readonly string _data = Directory.CreateTempSubdirectory("cna-isostore-").FullName;
    private readonly string? _previousData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

    public IsolatedStorageFileTests()
    {
        // The store lives under LocalApplicationData; keep it out of the developer's own.
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", _data);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", _previousData);
        Directory.Delete(_data, recursive: true);
    }

    [Fact]
    public void UserStoreForApplication_ReportsDotNetsScopeAndUnlimitedQuota()
    {
        using var store = IsolatedStorageFile.GetUserStoreForApplication();

        Assert.Equal(IsolatedStorageScope.User | IsolatedStorageScope.Application, store.Scope);
        Assert.Equal(long.MaxValue, store.Quota);
        Assert.Equal(long.MaxValue, store.AvailableFreeSpace);
        Assert.Equal(0, store.UsedSize);
        Assert.True(store.IncreaseQuotaTo(long.MaxValue));
        Assert.True(IsolatedStorageFile.IsEnabled);
    }

    [Fact]
    public void BackslashSeparatesDirectories_AsOnWindowsPhone()
    {
        using var store = IsolatedStorageFile.GetUserStoreForApplication();
        store.CreateDirectory("ScreenManager");
        using (var stream = store.CreateFile("ScreenManager\\ScreenList.dat"))
        {
            stream.WriteByte(42);
        }

        Assert.True(store.FileExists("ScreenManager\\ScreenList.dat"));
        Assert.True(store.FileExists("ScreenManager/ScreenList.dat"));
        Assert.Equal(["ScreenList.dat"], store.GetFileNames("ScreenManager\\*"));
        Assert.Equal(["ScreenManager"], store.GetDirectoryNames());
        Assert.Empty(store.GetFileNames());
    }

    [Fact]
    public void WrittenBytes_ReadBack_AndAppendAppends()
    {
        using var store = IsolatedStorageFile.GetUserStoreForApplication();
        using (var stream = store.OpenFile("scores.dat", FileMode.Create))
        {
            Assert.IsAssignableFrom<FileStream>(stream);
            stream.Write([1, 2, 3]);
        }

        using (var stream = store.OpenFile("scores.dat", FileMode.Append))
        {
            stream.WriteByte(4);
        }

        using (var stream = new IsolatedStorageFileStream("scores.dat", FileMode.Open, FileAccess.Read, store))
        {
            var read = new byte[8];
            Assert.Equal(4, stream.Read(read));
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, read[..4]);
        }

        // Without a store, a stream opens in the application's user store.
        using var implicitStore = new IsolatedStorageFileStream("scores.dat", FileMode.Open);
        Assert.Equal(4, implicitStore.Length);
    }

    [Fact]
    public void Failures_AreDotNets()
    {
        using var store = IsolatedStorageFile.GetUserStoreForApplication();

        var open = Assert.Throws<IsolatedStorageException>(() => store.OpenFile("missing.dat", FileMode.Open));
        Assert.IsType<FileNotFoundException>(open.InnerException);
        Assert.Equal("Operation not permitted on IsolatedStorageFileStream.", open.Message);

        var inMissingDirectory = Assert.Throws<IsolatedStorageException>(() => store.CreateFile("nodir/x.dat"));
        Assert.IsType<DirectoryNotFoundException>(inMissingDirectory.InnerException);

        store.DeleteFile("missing.dat");

        var deleteDirectory = Assert.Throws<IsolatedStorageException>(() => store.DeleteDirectory("nodir"));
        Assert.IsType<DirectoryNotFoundException>(deleteDirectory.InnerException);

        store.CreateFile("a.dat").Dispose();
        var copy = Assert.Throws<IsolatedStorageException>(() => store.CopyFile("a.dat", "a.dat"));
        Assert.IsAssignableFrom<IOException>(copy.InnerException);
    }

    [Fact]
    public void RemovedOrDisposedStore_RefusesUse()
    {
        var removed = IsolatedStorageFile.GetUserStoreForApplication();
        removed.CreateFile("a.dat").Dispose();
        removed.Remove();
        Assert.Throws<InvalidOperationException>(() => removed.FileExists("a.dat"));
        Assert.False(IsolatedStorageFile.GetUserStoreForApplication().FileExists("a.dat"));

        var disposed = IsolatedStorageFile.GetUserStoreForApplication();
        disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => disposed.FileExists("a.dat"));
    }
}
