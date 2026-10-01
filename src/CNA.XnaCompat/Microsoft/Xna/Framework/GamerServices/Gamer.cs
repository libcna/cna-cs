using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's gamer, over a CNA gamer handle.
///
/// One managed object per native gamer, however many handles native hands out for it: XNA code
/// keeps references, compares them, and hangs state off <see cref="Tag"/>. Native gives every gamer
/// a caller-owned 64-bit tag for exactly this ("a pointer-sized value a caller can key its own table
/// with"), so the facade stamps each gamer it wraps with an identity and maps it back to the one
/// object. XNA's own <see cref="Tag"/> is managed state and never reaches native.
/// </summary>
public abstract unsafe class Gamer
{
    private static readonly object s_identityLock = new();
    private static readonly Dictionary<ulong, Gamer> s_byIdentity = new();
    private static ulong s_nextIdentity;

    private readonly NativeResourceHandle? _ownedHandle;
    private readonly CnaHandle _borrowedHandle;
    private readonly ulong _identity;

    /// <summary>
    /// Wraps <paramref name="handle"/>, releasing it with <paramref name="destroy"/> when
    /// <paramref name="ownsHandle"/>: a signed-in gamer, a network gamer and a plain gamer are
    /// different native kinds, and each kind's release route refuses the others'.
    /// </summary>
    internal Gamer(CnaHandle handle, bool ownsHandle, Func<CnaHandle, CnaResult> destroy)
    {
        if (ownsHandle)
        {
            _ownedHandle = new NativeResourceHandle(
                handle.Value, value => destroy(new CnaHandle(value)).IsSuccess());
        }
        else
        {
            _borrowedHandle = handle;
        }

        _identity = Interlocked.Increment(ref s_nextIdentity);
        GamerServicesInterop.Check(Native.cna_gamer_set_tag(handle, _identity), nameof(Gamer));
        lock (s_identityLock)
        {
            s_byIdentity[_identity] = this;
        }
    }

    internal CnaHandle Handle
    {
        get
        {
            if (_ownedHandle is not null)
            {
                return new CnaHandle(_ownedHandle.DangerousGetHandle());
            }

            return _borrowedHandle;
        }
    }

    /// <summary>
    /// The managed gamer for a native handle: the existing object when this gamer was wrapped before,
    /// otherwise a new one from <paramref name="create"/>. An owned handle to an already-wrapped gamer
    /// is released here through <paramref name="destroy"/>, because the existing object keeps its
    /// own.
    /// </summary>
    internal static T Wrap<T>(
        CnaHandle handle, bool ownsHandle, Func<CnaHandle, bool, T> create, Func<CnaHandle, CnaResult> destroy)
        where T : Gamer
    {
        GamerServicesInterop.Check(Native.cna_gamer_get_tag(handle, out ulong tag), nameof(Wrap));
        if (tag != 0)
        {
            Gamer? existing;
            lock (s_identityLock)
            {
                s_byIdentity.TryGetValue(tag, out existing);
            }

            if (existing is T typed)
            {
                if (ownsHandle)
                {
                    GamerServicesInterop.Check(destroy(handle), nameof(Wrap));
                }

                return typed;
            }
        }

        return create(handle, ownsHandle);
    }

    /// <summary>Forgets a gamer native has disposed, so the table does not keep it alive.</summary>
    internal void Forget()
    {
        lock (s_identityLock)
        {
            s_byIdentity.Remove(_identity);
        }
    }

    /// <summary>Forgets this gamer and releases its handle now, ahead of whatever owns the gamer
    /// natively (a network session refuses to be destroyed while a view of its gamers is open).</summary>
    internal void Release()
    {
        Forget();
        _ownedHandle?.Dispose();
    }

    /// <summary>
    /// The managed gamer a native handle names, found by the identity the facade stamped on it, or
    /// null when the facade has never wrapped that gamer. The handle itself is not kept.
    /// </summary>
    internal static Gamer? FromHandle(CnaHandle handle)
    {
        if (handle.IsNull)
        {
            return null;
        }

        GamerServicesInterop.Check(Native.cna_gamer_get_tag(handle, out ulong tag), nameof(FromHandle));
        if (tag == 0)
        {
            return null;
        }

        lock (s_identityLock)
        {
            return s_byIdentity.TryGetValue(tag, out Gamer? gamer) ? gamer : null;
        }
    }

    public string Gamertag => GamerServicesInterop.ReadString(
        Native.cna_gamer_get_gamertag_size, CopyGamertag, Handle, nameof(Gamertag));

    public string DisplayName
    {
        get => GamerServicesInterop.ReadString(
            Native.cna_gamer_get_display_name_size, CopyDisplayName, Handle, nameof(DisplayName));
        internal set => GamerServicesInterop.Check(
            GamerServicesInterop.WithString(value, view => Native.cna_gamer_set_display_name(Handle, view)), nameof(DisplayName));
    }

    public object Tag { get; set; } = null!;

    public bool IsDisposed
    {
        get
        {
            if (_ownedHandle is { IsClosed: true })
            {
                return true;
            }

            return Native.cna_gamer_get_is_disposed(Handle, out byte disposed) is not CnaResult.Success || disposed != 0;
        }
    }

    public static SignedInGamerCollection SignedInGamers => SignedInGamerCollection.Current;

    public override string ToString() => Gamertag;

    public GamerProfile GetProfile()
    {
        GamerServicesInterop.Check(Native.cna_gamer_get_profile(Handle, out CnaHandle profile), nameof(GetProfile));
        return new GamerProfile(profile);
    }

    public IAsyncResult BeginGetProfile(AsyncCallback callback, object asyncState)
    {
        var result = new GamerServicesAsyncResult(callback, asyncState, s_getProfileOwner);
        // Native completes this Begin before returning, so no native callback is passed: the managed
        // result completes once the payload is stored, never before (see GamerServicesAsyncResult).
        CnaResult status = Native.cna_gamer_begin_get_profile(Handle, 0, 0, out CnaHandle profile);
        GamerServicesInterop.Check(status, nameof(BeginGetProfile));
        result.Payload = profile;
        result.Complete(synchronously: true);
        return result;
    }

    public GamerProfile EndGetProfile(IAsyncResult result)
    {
        GamerServicesAsyncResult ours = GamerServicesAsyncResult.ForEnd(result, s_getProfileOwner);
        return new GamerProfile((CnaHandle)ours.Payload!);
    }

    public LeaderboardWriter LeaderboardWriter => GetLeaderboardWriter();

    /// <summary>XNA's own base throws: only a gamer in a session that writes leaderboards has one.</summary>
    internal virtual LeaderboardWriter GetLeaderboardWriter() =>
        throw new NotSupportedException("Leaderboards are written through a network session's gamers.");

    public static IAsyncResult BeginGetFromGamertag(string gamertag, AsyncCallback callback, object asyncState)
    {
        ArgumentNullException.ThrowIfNull(gamertag);
        var result = new GamerServicesAsyncResult(callback, asyncState, s_getFromGamertagOwner);
        CnaHandle gamer = CnaHandle.Zero;
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(gamertag, view => Native.cna_gamer_begin_get_from_gamertag(
                view, 0, 0, out gamer)),
            nameof(BeginGetFromGamertag));
        result.Payload = gamer;
        result.Complete(synchronously: true);
        return result;
    }

    public static Gamer EndGetFromGamertag(IAsyncResult result)
    {
        GamerServicesAsyncResult ours = GamerServicesAsyncResult.ForEnd(result, s_getFromGamertagOwner);
        return Wrap((CnaHandle)ours.Payload!, ownsHandle: true, static (h, owned) => new RemoteGamer(h, owned), Native.cna_gamer_destroy);
    }

    public static Gamer GetFromGamertag(string gamertag)
    {
        ArgumentNullException.ThrowIfNull(gamertag);
        CnaHandle gamer = CnaHandle.Zero;
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(gamertag, view => Native.cna_gamer_get_from_gamertag(view, out gamer)),
            nameof(GetFromGamertag));
        return Wrap(gamer, ownsHandle: true, static (h, owned) => new RemoteGamer(h, owned), Native.cna_gamer_destroy);
    }

    public static IAsyncResult BeginGetPartnerToken(string audienceUri, AsyncCallback callback, object asyncState)
    {
        ArgumentNullException.ThrowIfNull(audienceUri);
        var result = new GamerServicesAsyncResult(callback, asyncState, s_partnerTokenOwner);
        result.Payload = GetPartnerToken(audienceUri);
        result.Complete(synchronously: true);
        return result;
    }

    public static string EndGetPartnerToken(IAsyncResult result) =>
        (string)GamerServicesAsyncResult.ForEnd(result, s_partnerTokenOwner).Payload!;

    public static unsafe string GetPartnerToken(string audienceUri)
    {
        ArgumentNullException.ThrowIfNull(audienceUri);
        ulong size = 0;
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(audienceUri, view => Native.cna_gamer_get_partner_token_size(view, out size)),
            nameof(GetPartnerToken));
        byte[] buffer = new byte[size];
        ulong written = 0;
        GamerServicesInterop.Check(
            GamerServicesInterop.WithString(audienceUri, view =>
            {
                fixed (byte* pointer = buffer)
                {
                    return Native.cna_gamer_copy_partner_token(view, pointer, size, out written);
                }
            }),
            nameof(GetPartnerToken));
        return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);
    }

    private static unsafe CnaResult CopyGamertag(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_gamer_copy_gamertag(handle, destination, capacity, out bytes);

    private static unsafe CnaResult CopyDisplayName(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_gamer_copy_display_name(handle, destination, capacity, out bytes);

    private static readonly object s_getProfileOwner = new();
    private static readonly object s_getFromGamertagOwner = new();
    private static readonly object s_partnerTokenOwner = new();
}

/// <summary>A gamer looked up by gamertag: XNA returns one through the abstract base type.</summary>
internal sealed class RemoteGamer : Gamer
{
    internal RemoteGamer(CnaHandle handle, bool ownsHandle)
        : base(handle, ownsHandle, Native.cna_gamer_destroy)
    {
    }
}
