using CNA.Interop;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// A gamer in a network session, over the session's view of that native gamer. Its session wraps it
/// once and keeps the object for as long as native keeps the gamer -- through leaving, into
/// <see cref="NetworkSession.PreviousGamers"/> -- and releases the view before the session itself.
/// </summary>
public class NetworkGamer : Gamer
{
    internal NetworkGamer(NetworkSession session, CnaHandle handle)
        : base(handle, ownsHandle: true, Native.cna_network_gamer_destroy)
    {
        Session = session;
    }

    public NetworkSession Session { get; }

    /// <summary>XNA's internal setter is its session's, assigning the machine a gamer arrived on;
    /// here native knows the machine, so nothing in this facade sets it.</summary>
    public NetworkMachine Machine
    {
        get => Session.MachineOf(this);
        internal set => throw new InvalidOperationException("A gamer's machine is the session's own state.");
    }

    public bool IsHost => Flag(Native.cna_network_gamer_get_is_host, nameof(IsHost));

    public bool IsLocal => Flag(Native.cna_network_gamer_get_is_local, nameof(IsLocal));

    public bool IsPrivateSlot => Flag(Native.cna_network_gamer_get_is_private_slot, nameof(IsPrivateSlot));

    public bool IsReady
    {
        get => Flag(Native.cna_network_gamer_get_is_ready, nameof(IsReady));
        set => GamerServicesInterop.Check(
            Native.cna_network_gamer_set_is_ready(Handle, GamerServicesInterop.Bool(value)), nameof(IsReady));
    }

    public bool HasVoice => Flag(Native.cna_network_gamer_get_has_voice, nameof(HasVoice));

    public bool IsTalking => Flag(Native.cna_network_gamer_get_is_talking, nameof(IsTalking));

    public bool IsMutedByLocalUser => Flag(Native.cna_network_gamer_get_is_muted_by_local_user, nameof(IsMutedByLocalUser));

    public bool IsGuest => Flag(Native.cna_network_gamer_get_is_guest, nameof(IsGuest));

    public TimeSpan RoundtripTime
    {
        get
        {
            GamerServicesInterop.Check(Native.cna_network_gamer_get_roundtrip_ticks(Handle, out long ticks), nameof(RoundtripTime));
            return new TimeSpan(ticks);
        }
    }

    public unsafe byte Id
    {
        get
        {
            byte id;
            GamerServicesInterop.Check(Native.cna_network_gamer_get_id(Handle, &id), nameof(Id));
            return id;
        }
    }

    public bool HasLeftSession => Flag(Native.cna_network_gamer_get_has_left_session, nameof(HasLeftSession));

    private protected delegate CnaResult FlagQuery(CnaHandle gamer, out byte value);

    private protected bool Flag(FlagQuery query, string operation)
    {
        GamerServicesInterop.Check(query(Handle, out byte value), operation);
        return value != 0;
    }
}
