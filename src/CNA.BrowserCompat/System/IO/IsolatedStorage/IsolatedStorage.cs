namespace System.IO.IsolatedStorage;

/// <summary>The base of every isolated store, as .NET defines it. A browser has no quota for
/// one, and reports none, as .NET does on every platform it implements this for.</summary>
public abstract class IsolatedStorage : MarshalByRefObject
{
    public IsolatedStorageScope Scope { get; private protected set; }

    public virtual long AvailableFreeSpace => long.MaxValue;

    public virtual long Quota => long.MaxValue;

    public virtual long UsedSize => 0;

    protected virtual char SeparatorExternal => Path.DirectorySeparatorChar;

    protected virtual char SeparatorInternal => '.';

    public virtual bool IncreaseQuotaTo(long newQuotaSize) => true;

    public abstract void Remove();
}
