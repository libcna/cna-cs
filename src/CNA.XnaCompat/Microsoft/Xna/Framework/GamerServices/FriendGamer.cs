using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>A friend in a <see cref="FriendCollection"/>; its handle is the collection's.</summary>
public sealed unsafe class FriendGamer : Gamer
{
    internal FriendGamer(CnaHandle handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public string Presence => GamerServicesInterop.ReadString(
        Native.cna_friend_gamer_get_presence_size, CopyPresence, Handle, nameof(Presence));

    public bool IsOnline => Info().IsOnline != 0;

    public bool IsPlaying => Info().IsPlaying != 0;

    public bool IsJoinable => Info().IsJoinable != 0;

    public bool IsAway => Info().IsAway != 0;

    public bool IsBusy => Info().IsBusy != 0;

    public bool HasVoice => Info().HasVoice != 0;

    public bool FriendRequestReceivedFrom => Info().FriendRequestReceivedFrom != 0;

    public bool FriendRequestSentTo => Info().FriendRequestSentTo != 0;

    public bool InviteReceivedFrom => Info().InviteReceivedFrom != 0;

    public bool InviteSentTo => Info().InviteSentTo != 0;

    public bool InviteAccepted => Info().InviteAccepted != 0;

    public bool InviteRejected => Info().InviteRejected != 0;

    private CnaFriendGamerInfo Info()
    {
        CnaFriendGamerInfo info = SignedInGamer.Versioned<CnaFriendGamerInfo>();
        GamerServicesInterop.Check(Native.cna_friend_gamer_get_info(Handle, ref info), nameof(FriendGamer));
        return info;
    }

    private static unsafe CnaResult CopyPresence(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_friend_gamer_copy_presence(handle, destination, capacity, out bytes);
}
