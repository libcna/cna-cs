namespace System.IO.IsolatedStorage;

/// <summary>The levels of scope an isolated store is opened at, as .NET defines them.</summary>
[Flags]
public enum IsolatedStorageScope
{
    None = 0,
    User = 1,
    Domain = 2,
    Assembly = 4,
    Roaming = 8,
    Machine = 16,
    Application = 32,
}
