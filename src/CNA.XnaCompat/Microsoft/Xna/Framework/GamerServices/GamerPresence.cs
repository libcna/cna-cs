using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>A signed-in gamer's live presence: every read and write goes to native.</summary>
public sealed class GamerPresence
{
    private readonly SignedInGamer _gamer;

    internal GamerPresence(SignedInGamer gamer)
    {
        _gamer = gamer;
    }

    public GamerPresenceMode PresenceMode
    {
        get => (GamerPresenceMode)Read().PresenceMode;
        set
        {
            CnaGamerPresence presence = Read();
            presence.PresenceMode = (uint)value;
            Write(presence);
        }
    }

    public int PresenceValue
    {
        get => Read().PresenceValue;
        set
        {
            CnaGamerPresence presence = Read();
            presence.PresenceValue = value;
            Write(presence);
        }
    }

    private CnaGamerPresence Read()
    {
        CnaGamerPresence presence = SignedInGamer.Versioned<CnaGamerPresence>();
        GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_presence(_gamer.Handle, ref presence), nameof(GamerPresence));
        return presence;
    }

    private unsafe void Write(CnaGamerPresence presence) =>
        GamerServicesInterop.Check(Native.cna_signed_in_gamer_set_presence(_gamer.Handle, &presence), nameof(GamerPresence));
}
