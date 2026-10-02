using Microsoft.Win32.SafeHandles;

namespace System.IO.IsolatedStorage;

/// <summary>A file in an isolated store, as .NET's IsolatedStorageFileStream: a FileStream whose
/// failure to open surfaces as <see cref="IsolatedStorageException"/> with the IO exception
/// inside. Without a store it opens in the application's user store.</summary>
public class IsolatedStorageFileStream : FileStream
{
    private const int DefaultBufferSize = 4096;
    private const string OperationNotPermitted = "Operation not permitted on IsolatedStorageFileStream.";

    public IsolatedStorageFileStream(string path, FileMode mode)
        : this(path, mode, AccessFor(mode), FileShare.None, DefaultBufferSize, null)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, IsolatedStorageFile? isf)
        : this(path, mode, AccessFor(mode), FileShare.None, DefaultBufferSize, isf)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access)
        : this(path, mode, access, FileShare.None, DefaultBufferSize, null)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, IsolatedStorageFile? isf)
        : this(path, mode, access, FileShare.None, DefaultBufferSize, isf)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        : this(path, mode, access, share, DefaultBufferSize, null)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile? isf)
        : this(path, mode, access, share, DefaultBufferSize, isf)
    {
    }

    public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        : this(path, mode, access, share, bufferSize, null)
    {
    }

    public IsolatedStorageFileStream(
        string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile? isf)
        : base(Open(path, mode, access, share, isf), access, bufferSize)
    {
        if (mode == FileMode.Append)
        {
            Seek(0, SeekOrigin.End);
        }
    }

    private static FileAccess AccessFor(FileMode mode) => mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite;

    private static SafeFileHandle Open(string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile? isf)
    {
        ArgumentNullException.ThrowIfNull(path);
        string full;
        if (isf is null)
        {
            using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            full = store.FullPath(path);
        }
        else
        {
            full = isf.FullPath(path);
        }

        try
        {
            return File.OpenHandle(full, mode, access, share);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new IsolatedStorageException(OperationNotPermitted, exception);
        }
    }
}
