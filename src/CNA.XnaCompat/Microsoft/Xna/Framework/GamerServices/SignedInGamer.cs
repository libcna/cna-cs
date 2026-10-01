using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CNA.Interop;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed class SignedInGamer : Gamer
{
    private static EventHandler<SignedInEventArgs>? s_signedIn;
    private static EventHandler<SignedOutEventArgs>? s_signedOut;
    private static CnaHandle s_signedInRegistration;
    private static CnaHandle s_signedOutRegistration;
    private static readonly object s_awardOwner = new();
    private static readonly object s_achievementsOwner = new();

    private GamerPresence? _presence;

    internal SignedInGamer(CnaHandle handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public PlayerIndex PlayerIndex
    {
        get
        {
            GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_player_index(Handle, out uint index), nameof(PlayerIndex));
            return (PlayerIndex)index;
        }
    }

    public bool IsSignedInToLive => Flag(Native.cna_signed_in_gamer_get_is_signed_in_to_live, nameof(IsSignedInToLive));

    public bool IsGuest => Flag(Native.cna_signed_in_gamer_get_is_guest, nameof(IsGuest));

    public GameDefaults GameDefaults
    {
        get
        {
            CnaGameDefaults defaults = GamerServicesInterop.Versioned<CnaGameDefaults>();
            GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_game_defaults(Handle, ref defaults), nameof(GameDefaults));
            return new GameDefaults(defaults);
        }
    }

    public GamerPrivileges Privileges
    {
        get
        {
            CnaGamerPrivileges privileges = GamerServicesInterop.Versioned<CnaGamerPrivileges>();
            GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_privileges(Handle, ref privileges), nameof(Privileges));
            return new GamerPrivileges(privileges);
        }
    }

    public int PartySize
    {
        get
        {
            GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_party_size(Handle, out int size), nameof(PartySize));
            return size;
        }
        internal set => GamerServicesInterop.Check(Native.cna_signed_in_gamer_set_party_size(Handle, value), nameof(PartySize));
    }

    public GamerPresence Presence => _presence ??= new GamerPresence(this);

    public static event EventHandler<SignedInEventArgs> SignedIn
    {
        add
        {
            if (s_signedIn is null && value is not null)
            {
                s_signedInRegistration = Subscribe(signedIn: true);
            }

            s_signedIn += value;
        }
        remove
        {
            s_signedIn -= value;
            if (s_signedIn is null)
            {
                Unsubscribe(ref s_signedInRegistration);
            }
        }
    }

    public static event EventHandler<SignedOutEventArgs> SignedOut
    {
        add
        {
            if (s_signedOut is null && value is not null)
            {
                s_signedOutRegistration = Subscribe(signedIn: false);
            }

            s_signedOut += value;
        }
        remove
        {
            s_signedOut -= value;
            if (s_signedOut is null)
            {
                Unsubscribe(ref s_signedOutRegistration);
            }
        }
    }

    public bool IsFriend(Gamer gamer)
    {
        ArgumentNullException.ThrowIfNull(gamer);
        GamerServicesInterop.Check(Native.cna_signed_in_gamer_is_friend(Handle, gamer.Handle, out byte value), nameof(IsFriend));
        return value != 0;
    }

    public FriendCollection GetFriends()
    {
        GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_friends(Handle, out CnaHandle friends), nameof(GetFriends));
        return new FriendCollection(friends);
    }

    public bool IsHeadset(Microphone microphone)
    {
        ArgumentNullException.ThrowIfNull(microphone);
        GamerServicesInterop.Check(
            Native.cna_signed_in_gamer_is_headset(Handle, microphone.NativeIndex, out byte value), nameof(IsHeadset));
        return value != 0;
    }

    public IAsyncResult BeginAwardAchievement(string achievementKey, AsyncCallback callback, object state)
    {
        ArgumentNullException.ThrowIfNull(achievementKey);
        var result = new GamerServicesAsyncResult(callback, state, s_awardOwner);
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(achievementKey, view => Native.cna_signed_in_gamer_begin_award_achievement(Handle, view, 0, 0)),
            nameof(BeginAwardAchievement));
        result.Complete(synchronously: true);
        return result;
    }

    public void EndAwardAchievement(IAsyncResult result) => GamerServicesAsyncResult.ForEnd(result, s_awardOwner);

    public void AwardAchievement(string achievementKey)
    {
        ArgumentNullException.ThrowIfNull(achievementKey);
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(achievementKey, view => Native.cna_signed_in_gamer_award_achievement(Handle, view)),
            nameof(AwardAchievement));
    }

    public IAsyncResult BeginGetAchievements(AsyncCallback callback, object asyncState)
    {
        var result = new GamerServicesAsyncResult(callback, asyncState, s_achievementsOwner);
        GamerServicesInterop.Check(
            Native.cna_signed_in_gamer_begin_get_achievements(Handle, 0, 0, out CnaHandle achievements), nameof(BeginGetAchievements));
        result.Payload = achievements;
        result.Complete(synchronously: true);
        return result;
    }

    public AchievementCollection EndGetAchievements(IAsyncResult result) =>
        new((CnaHandle)GamerServicesAsyncResult.ForEnd(result, s_achievementsOwner).Payload!);

    public AchievementCollection GetAchievements()
    {
        GamerServicesInterop.Check(Native.cna_signed_in_gamer_get_achievements(Handle, out CnaHandle achievements), nameof(GetAchievements));
        return new AchievementCollection(achievements);
    }

    private delegate CnaResult BoolQuery(CnaHandle gamer, out byte value);

    private bool Flag(BoolQuery query, string operation)
    {
        GamerServicesInterop.Check(query(Handle, out byte value), operation);
        return value != 0;
    }

    private static unsafe CnaHandle Subscribe(bool signedIn)
    {
        nint callback = signedIn
            ? (nint)(delegate* unmanaged[Cdecl]<nint, CnaSignedInGamerEventInfo*, void>)&OnSignedIn
            : (nint)(delegate* unmanaged[Cdecl]<nint, CnaSignedInGamerEventInfo*, void>)&OnSignedOut;
        CnaHandle registration;
        CnaResult result = signedIn
            ? Native.cna_signed_in_gamer_subscribe_signed_in_ext(callback, 0, out registration)
            : Native.cna_signed_in_gamer_subscribe_signed_out_ext(callback, 0, out registration);
        GamerServicesInterop.Check(result, signedIn ? nameof(SignedIn) : nameof(SignedOut));
        return registration;
    }

    private static void Unsubscribe(ref CnaHandle registration)
    {
        if (!registration.IsNull)
        {
            _ = Native.cna_gamer_unsubscribe_ext(registration);
            registration = CnaHandle.Zero;
        }
    }

    /// <summary>
    /// The managed gamer an event names. The handle native passes is borrowed for the callback, so
    /// a gamer this facade has not seen yet is found through the signed-in collection, which wraps
    /// it with a handle of its own; one that has already left keeps the object it had.
    /// </summary>
    private static unsafe SignedInGamer Resolve(CnaSignedInGamerEventInfo* info)
    {
        CnaHandle handle = info->Gamer;
        if (Native.cna_gamer_get_tag(handle, out ulong tag).IsSuccess() && tag == 0)
        {
            SignedInGamerCollection.Refresh();
        }

        return Wrap(handle, ownsHandle: false, static (h, owned) => new SignedInGamer(h, owned));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnSignedIn(nint context, CnaSignedInGamerEventInfo* info)
    {
        try
        {
            s_signedIn?.Invoke(null, new SignedInEventArgs(Resolve(info)));
        }
        catch (Exception exception)
        {
            GamerServicesInterop.ReportCallbackException(exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnSignedOut(nint context, CnaSignedInGamerEventInfo* info)
    {
        try
        {
            SignedInGamer gamer = Resolve(info);
            s_signedOut?.Invoke(null, new SignedOutEventArgs(gamer));
        }
        catch (Exception exception)
        {
            GamerServicesInterop.ReportCallbackException(exception);
        }
    }
}
