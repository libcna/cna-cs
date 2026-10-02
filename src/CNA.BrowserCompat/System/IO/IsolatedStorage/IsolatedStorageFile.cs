using System.Reflection;

namespace System.IO.IsolatedStorage;

/// <summary>
/// An isolated store: a directory of the application's own, as .NET's IsolatedStorageFile is.
///
/// <b>Behaviour is .NET's, measured on the desktop</b> (the runtime CNA.NET games use everywhere
/// else): every store reports an unlimited quota and no used size; deleting a file that does not
/// exist is not an error; an IO failure surfaces as <see cref="IsolatedStorageException"/> with the
/// IO exception inside; a closed or removed store refuses further use.
///
/// <b>One deliberate difference: a backslash separates directories.</b> Windows Phone and Windows,
/// the platforms XNA games were written for, read <c>"ScreenManager\\ScreenList.dat"</c> as a file
/// in a directory; .NET on a Unix file system reads it as one file whose name contains a backslash.
///
/// The store lives in the browser's virtual file system, so it lasts as long as the page.
/// </summary>
public sealed class IsolatedStorageFile : IsolatedStorage, IDisposable
{
    private const string StoreNotOpen = "Store must be open for this operation.";

    private readonly string _root;
    private bool _closed;
    private bool _disposed;

    private IsolatedStorageFile(IsolatedStorageScope scope)
    {
        Scope = scope;
        _root = RootFor(scope);
        Directory.CreateDirectory(_root);
    }

    /// <summary>Isolated storage is available.</summary>
    public static bool IsEnabled => true;

    public static IsolatedStorageFile GetUserStoreForApplication() =>
        new(IsolatedStorageScope.User | IsolatedStorageScope.Application);

    public static IsolatedStorageFile GetUserStoreForAssembly() =>
        new(IsolatedStorageScope.User | IsolatedStorageScope.Assembly);

    public static IsolatedStorageFile GetUserStoreForDomain() =>
        new(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly);

    public static IsolatedStorageFile GetMachineStoreForApplication() =>
        new(IsolatedStorageScope.Machine | IsolatedStorageScope.Application);

    public static IsolatedStorageFile GetMachineStoreForAssembly() =>
        new(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly);

    public static IsolatedStorageFile GetMachineStoreForDomain() =>
        new(IsolatedStorageScope.Machine | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly);

    public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Type? domainEvidenceType, Type? assemblyEvidenceType) =>
        new(scope);

    public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, object? domainIdentity, object? assemblyIdentity) =>
        new(scope);

    /// <summary>Deletes every store of the given scope.</summary>
    public static void Remove(IsolatedStorageScope scope)
    {
        string root = RootFor(scope);
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    public bool FileExists(string path) => File.Exists(FullPath(path));

    public bool DirectoryExists(string path) => Directory.Exists(FullPath(path));

    public void CreateDirectory(string dir) =>
        Wrap(() => Directory.CreateDirectory(FullPath(dir)), "Unable to create directory.");

    public void DeleteFile(string file) => Wrap(() => File.Delete(FullPath(file)), "Unable to delete file.");

    public void DeleteDirectory(string dir) =>
        Wrap(() => Directory.Delete(FullPath(dir), recursive: false), "Unable to delete, directory not empty or does not exist.");

    public string[] GetFileNames() => GetFileNames("*");

    public string[] GetFileNames(string searchPattern) => Names(searchPattern, Directory.GetFiles);

    public string[] GetDirectoryNames() => GetDirectoryNames("*");

    public string[] GetDirectoryNames(string searchPattern) => Names(searchPattern, Directory.GetDirectories);

    public IsolatedStorageFileStream CreateFile(string path) =>
        new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, this);

    public IsolatedStorageFileStream OpenFile(string path, FileMode mode) => new(path, mode, this);

    public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access) =>
        new(path, mode, access, this);

    public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share) =>
        new(path, mode, access, share, this);

    public void CopyFile(string sourceFileName, string destinationFileName) =>
        CopyFile(sourceFileName, destinationFileName, overwrite: false);

    public void CopyFile(string sourceFileName, string destinationFileName, bool overwrite) =>
        Wrap(() => File.Copy(FullPath(sourceFileName), FullPath(destinationFileName), overwrite), "Operation not permitted.");

    public void MoveFile(string sourceFileName, string destinationFileName) =>
        Wrap(() => File.Move(FullPath(sourceFileName), FullPath(destinationFileName)), "Operation not permitted.");

    public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName) =>
        Wrap(() => Directory.Move(FullPath(sourceDirectoryName), FullPath(destinationDirectoryName)), "Operation not permitted.");

    public DateTimeOffset GetCreationTime(string path) => File.GetCreationTime(FullPath(path));

    public DateTimeOffset GetLastAccessTime(string path) => File.GetLastAccessTime(FullPath(path));

    public DateTimeOffset GetLastWriteTime(string path) => File.GetLastWriteTime(FullPath(path));

    /// <summary>Deletes this store and everything in it; the store cannot be used afterwards.</summary>
    public override void Remove()
    {
        EnsureOpen();
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }

        _closed = true;
    }

    public void Close() => _closed = true;

    public void Dispose()
    {
        _closed = true;
        _disposed = true;
    }

    /// <summary>The host path of a path inside this store.</summary>
    internal string FullPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        EnsureOpen();
        return Path.Combine(_root, path.Replace('\\', '/').TrimStart('/'));
    }

    private string[] Names(string searchPattern, Func<string, string, string[]> list)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);
        string full = FullPath(searchPattern);
        string directory = Path.GetDirectoryName(full) ?? _root;
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return list(directory, Path.GetFileName(full)).Select(name => Path.GetFileName(name)).ToArray();
    }

    private void EnsureOpen()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_closed)
        {
            throw new InvalidOperationException(StoreNotOpen);
        }
    }

    private static void Wrap(Action operation, string message)
    {
        try
        {
            operation();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new IsolatedStorageException(message, exception);
        }
    }

    private static string RootFor(IsolatedStorageScope scope)
    {
        string data = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
        if (string.IsNullOrEmpty(data))
        {
            data = Path.Combine(Path.GetTempPath(), "cna");
        }

        string application = Assembly.GetEntryAssembly()?.GetName().Name ?? "CNA";
        string level = (scope & IsolatedStorageScope.Machine) != 0 ? "Machine" : "User";
        return Path.Combine(data, "IsolatedStorage", level, application);
    }
}
