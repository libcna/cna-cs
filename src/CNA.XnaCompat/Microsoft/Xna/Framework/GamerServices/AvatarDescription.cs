using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's avatar description, over a CNA avatar description handle.
///
/// XNA keeps one description per signed-in player: <see cref="EndGetFromGamer"/> returns that
/// object until the player's avatar changes, when its <see cref="Changed"/> is raised (sender: the
/// gamer) and the slot is emptied so the next read returns a new one. Native shares the event of
/// that player's description among every copy it hands out, so the slot object subscribes once,
/// at creation, and the subscription lives exactly as long as its handle.
/// </summary>
public class AvatarDescription
{
    private const int DescriptionSize = 1021;

    private static readonly AvatarDescription?[] s_signedInGamerDescriptions = new AvatarDescription?[4];
    private static readonly object s_getFromGamerOwner = new();

    private readonly NativeResourceHandle _handle;
    private readonly Gamer? _gamer;
    private readonly int _slot = -1;

    public AvatarDescription(byte[] data)
        : this(Create(data))
    {
    }

    private AvatarDescription(CnaHandle handle, Gamer? gamer = null, int slot = -1)
    {
        _gamer = gamer;
        _slot = slot;
        if (slot < 0)
        {
            _handle = new NativeResourceHandle(
                handle.AsNint, value => Native.cna_avatar_description_destroy(new CnaHandle(value)).IsSuccess());
            return;
        }

        var weakSelf = GCHandle.Alloc(this, GCHandleType.Weak);
        CnaHandle registration = CnaHandle.Zero;
        try
        {
            GamerServicesInterop.Check(
                SubscribeChanged(handle, GCHandle.ToIntPtr(weakSelf), out registration), nameof(Changed));
        }
        catch
        {
            weakSelf.Free();
            _ = Native.cna_avatar_description_destroy(handle);
            throw;
        }

        // The registration borrows the description, so it is released first.
        _handle = new NativeResourceHandle(handle.AsNint, value =>
        {
            if (!registration.IsNull && !Native.cna_gamer_unsubscribe_ext(registration).IsSuccess())
            {
                return false;
            }

            registration = CnaHandle.Zero;
            if (weakSelf.IsAllocated)
            {
                weakSelf.Free();
            }

            return Native.cna_avatar_description_destroy(new CnaHandle(value)).IsSuccess();
        });
    }

    internal CnaHandle Handle => new(_handle.DangerousGetHandle());

    public event EventHandler<EventArgs> Changed = null!;

    public bool IsValid => Info().IsValid != 0;

    /// <summary>A copy of the description's bytes, as XNA returns.</summary>
    public unsafe byte[] Description
    {
        get
        {
            byte[] bytes = new byte[Info().DescriptionByteCount];
            fixed (byte* pointer = bytes)
            {
                GamerServicesInterop.Check(
                    Native.cna_avatar_description_copy_description(Handle, pointer, (ulong)bytes.Length, out ulong written),
                    nameof(Description));
                return written == (ulong)bytes.Length ? bytes : bytes[..(int)written];
            }
        }
    }

    public float Height => Info().Height;

    public AvatarBodyType BodyType => (AvatarBodyType)Info().BodyType;

    public static IAsyncResult BeginGetFromGamer(Gamer gamer, AsyncCallback callback, object state)
    {
        ArgumentNullException.ThrowIfNull(gamer);
        if (gamer.IsDisposed)
        {
            throw new ObjectDisposedException(gamer.GetType().Name);
        }

        int slot = gamer is SignedInGamer signedIn && (int)signedIn.PlayerIndex is >= 0 and < 4
            ? (int)signedIn.PlayerIndex
            : -1;
        var result = new GamerServicesAsyncResult(callback, state, s_getFromGamerOwner);
        if (slot < 0 || s_signedInGamerDescriptions[slot] is null)
        {
            // Native answers before returning; no native callback, so the managed result completes
            // only once the payload is stored.
            GamerServicesInterop.Check(
                Native.cna_avatar_description_get_from_gamer(gamer.Handle, 0, 0, out CnaHandle description),
                nameof(BeginGetFromGamer));
            result.Payload = new PendingDescription(description, gamer, slot);
        }
        else
        {
            result.Payload = new PendingDescription(CnaHandle.Zero, gamer, slot);
        }

        result.Complete(synchronously: true);
        return result;
    }

    /// <summary>
    /// A signed-in player's description is that player's one object until the avatar changes; any
    /// other gamer's is a new description. XNA lets this be called more than once for a result.
    /// </summary>
    public static AvatarDescription EndGetFromGamer(IAsyncResult result)
    {
        GamerServicesAsyncResult ours = GamerServicesAsyncResult.ForRepeatableEnd(result, s_getFromGamerOwner);
        var pending = (PendingDescription)ours.Payload!;
        if (pending.Slot >= 0 && s_signedInGamerDescriptions[pending.Slot] is { } cached)
        {
            pending.Release();
            return cached;
        }

        if (pending.Result is { } previous)
        {
            return previous;
        }

        var description = new AvatarDescription(pending.Take(), pending.Gamer, pending.Slot);
        if (pending.Slot >= 0)
        {
            s_signedInGamerDescriptions[pending.Slot] = description;
        }

        pending.Result = description;
        return description;
    }

    public static AvatarDescription CreateRandom()
    {
        GamerServicesInterop.Check(Native.cna_avatar_description_create_random(out CnaHandle handle), nameof(CreateRandom));
        return new AvatarDescription(handle);
    }

    public static AvatarDescription CreateRandom(AvatarBodyType bodyType)
    {
        if (bodyType is < AvatarBodyType.Female or > AvatarBodyType.Male)
        {
            throw new ArgumentOutOfRangeException(nameof(bodyType));
        }

        GamerServicesInterop.Check(
            Native.cna_avatar_description_create_random_for_body_type((uint)bodyType, out CnaHandle handle), nameof(CreateRandom));
        return new AvatarDescription(handle);
    }

    private CnaAvatarDescriptionInfo Info()
    {
        CnaAvatarDescriptionInfo info = GamerServicesInterop.Versioned<CnaAvatarDescriptionInfo>();
        GamerServicesInterop.Check(Native.cna_avatar_description_get_info(Handle, ref info), nameof(AvatarDescription));
        return info;
    }

    private static unsafe CnaHandle Create(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != DescriptionSize)
        {
            throw new ArgumentException($"The avatar description must be exactly {DescriptionSize} bytes.", nameof(data));
        }

        fixed (byte* pointer = data)
        {
            GamerServicesInterop.Check(
                Native.cna_avatar_description_create(pointer, (ulong)data.Length, out CnaHandle handle), nameof(AvatarDescription));
            return handle;
        }
    }

    private static unsafe CnaResult SubscribeChanged(CnaHandle description, nint context, out CnaHandle registration) =>
        Native.cna_avatar_description_subscribe_changed_ext(
            description, (nint)(delegate* unmanaged[Cdecl]<nint, void>)&OnNativeChanged, context, out registration);

    /// <summary>XNA's <c>OnAvatarChanged</c>: empty the player's slot, then raise on the old object.</summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnNativeChanged(nint context)
    {
        if (GCHandle.FromIntPtr(context).Target is not AvatarDescription description)
        {
            return;
        }

        if (description._slot >= 0 && ReferenceEquals(s_signedInGamerDescriptions[description._slot], description))
        {
            s_signedInGamerDescriptions[description._slot] = null;
        }

        try
        {
            description.Changed?.Invoke(description._gamer, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            GamerServicesInterop.ReportCallbackException(exception);
        }
    }

    /// <summary>What a Begin read for its End: an owned native description not yet wrapped.</summary>
    private sealed class PendingDescription(CnaHandle handle, Gamer gamer, int slot)
    {
        private CnaHandle _handle = handle;

        internal Gamer Gamer { get; } = gamer;

        internal int Slot { get; } = slot;

        internal AvatarDescription? Result { get; set; }

        internal CnaHandle Take()
        {
            CnaHandle handle = _handle;
            _handle = CnaHandle.Zero;
            return handle;
        }

        internal void Release()
        {
            if (!_handle.IsNull)
            {
                _ = Native.cna_avatar_description_destroy(Take());
            }
        }
    }
}
