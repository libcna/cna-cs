using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed class LeaderboardEntry
{
    private readonly NativeResourceHandle _handle;
    private PropertyDictionary? _columns;
    private Gamer? _gamer;

    internal LeaderboardEntry(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_leaderboard_entry_destroy(new CnaHandle(value)).IsSuccess());
    }

    internal CnaHandle Handle => new(_handle.DangerousGetHandle());

    public Gamer Gamer
    {
        get
        {
            if (_gamer is null)
            {
                GamerServicesInterop.Check(
                    Native.cna_leaderboard_entry_get_gamer(Handle, out byte hasGamer, out CnaHandle gamer), nameof(Gamer));
                if (hasGamer != 0)
                {
                    _gamer = GamerServices.Gamer.Wrap(gamer, ownsHandle: false, static (h, owned) => new RemoteGamer(h, owned), Native.cna_gamer_destroy);
                }
            }

            return _gamer!;
        }
    }

    public long Rating
    {
        get
        {
            CnaLeaderboardEntryInfo info = GamerServicesInterop.Versioned<CnaLeaderboardEntryInfo>();
            GamerServicesInterop.Check(Native.cna_leaderboard_entry_get_info(Handle, ref info), nameof(Rating));
            return info.Rating;
        }
        set => GamerServicesInterop.Check(Native.cna_leaderboard_entry_set_rating(Handle, value), nameof(Rating));
    }

    public PropertyDictionary Columns
    {
        get
        {
            if (_columns is null)
            {
                GamerServicesInterop.Check(Native.cna_leaderboard_entry_get_columns(Handle, out CnaHandle columns), nameof(Columns));
                _columns = new PropertyDictionary(columns);
            }

            return _columns;
        }
    }
}
