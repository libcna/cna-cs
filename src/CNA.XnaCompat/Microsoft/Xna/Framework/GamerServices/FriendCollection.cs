using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>A signed-in gamer's friends. Owns the native collection; its gamers borrow from it.</summary>
public sealed class FriendCollection : GamerCollection<FriendGamer>, IDisposable
{
    private readonly NativeResourceHandle _handle;

    internal FriendCollection(CnaHandle handle)
        : base(Load(handle))
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_gamer_collection_destroy(new CnaHandle(value)).IsSuccess());
    }

    ~FriendCollection()
    {
    }

    public bool IsDisposed => _handle.IsClosed;

    public void Dispose()
    {
        foreach (FriendGamer friend in this)
        {
            friend.Forget();
        }

        _handle.Dispose();
        GC.SuppressFinalize(this);
    }

    private static List<FriendGamer> Load(CnaHandle collection)
    {
        GamerServicesInterop.Check(Native.cna_gamer_collection_get_count(collection, out int count), nameof(FriendCollection));
        var friends = new List<FriendGamer>(count);
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(Native.cna_gamer_collection_get_at(collection, index, out CnaHandle gamer), nameof(FriendCollection));
            friends.Add(Gamer.Wrap(gamer, ownsHandle: false, static (h, owned) => new FriendGamer(h, owned)));
        }

        return friends;
    }
}
