namespace System.IO.IsolatedStorage;

/// <summary>The exception an isolated-storage operation throws, as .NET's does: the underlying
/// IO failure, when there is one, is its inner exception.</summary>
public class IsolatedStorageException : Exception
{
    public IsolatedStorageException()
        : base("An operation in isolated storage failed.")
    {
    }

    public IsolatedStorageException(string? message)
        : base(message)
    {
    }

    public IsolatedStorageException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
