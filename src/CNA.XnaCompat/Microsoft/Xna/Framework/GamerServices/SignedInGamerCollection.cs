using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// The process-wide signed-in gamers. One live object, as in XNA: its contents follow native's
/// collection, refreshed whenever it is reached, and each gamer is the same managed object for as
/// long as it stays signed in.
/// </summary>
public sealed class SignedInGamerCollection : GamerCollection<SignedInGamer>
{
    private static readonly List<SignedInGamer> s_gamers = new();
    private static readonly SignedInGamerCollection s_current = new();

    private SignedInGamerCollection()
        : base(s_gamers)
    {
    }

    internal static SignedInGamerCollection Current
    {
        get
        {
            Refresh();
            return s_current;
        }
    }

    public SignedInGamer this[PlayerIndex index]
    {
        get
        {
            Refresh();
            foreach (SignedInGamer gamer in s_gamers)
            {
                if (gamer.PlayerIndex == index)
                {
                    return gamer;
                }
            }

            return null!;
        }
    }

    /// <summary>Rebuilds the list from native, keeping each still-signed-in gamer's managed object.</summary>
    internal static void Refresh()
    {
        GamerServicesInterop.Check(Native.cna_gamer_get_signed_in_gamer_count(out int count), nameof(SignedInGamerCollection));
        var current = new List<SignedInGamer>(count);
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(Native.cna_gamer_get_signed_in_gamer_at(index, out CnaHandle handle), nameof(SignedInGamerCollection));
            current.Add(Gamer.Wrap(
                handle, ownsHandle: true, static (h, owned) => new SignedInGamer(h, owned), Native.cna_signed_in_gamer_destroy));
        }

        foreach (SignedInGamer previous in s_gamers)
        {
            if (!current.Contains(previous))
            {
                previous.Forget();
            }
        }

        s_gamers.Clear();
        s_gamers.AddRange(current);
    }
}
