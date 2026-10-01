using CNA;
using CNA.Interop;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// One session a search found. Like XNA's, its gamertag, counts and properties are the snapshot the
/// search returned; only the quality of service keeps arriving, into the same object.
/// </summary>
public sealed unsafe class AvailableNetworkSession
{
    private readonly NativeResourceHandle _handle;
    private readonly QualityOfService _qualityOfService = new();

    internal AvailableNetworkSession(CnaHandle handle, AvailableNetworkSessionCollection parent)
    {
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_available_network_session_destroy(new CnaHandle(value)).IsSuccess());
        Parent = parent;
        HostGamertag = GamerServicesInterop.ReadString(
            Native.cna_available_network_session_get_host_gamertag_size, CopyHostGamertag, Handle, nameof(HostGamertag));
        CurrentGamerCount = Read(Native.cna_available_network_session_get_current_gamer_count, nameof(CurrentGamerCount));
        OpenPublicGamerSlots = Read(Native.cna_available_network_session_get_open_public_gamer_slots, nameof(OpenPublicGamerSlots));
        OpenPrivateGamerSlots = Read(Native.cna_available_network_session_get_open_private_gamer_slots, nameof(OpenPrivateGamerSlots));

        GamerServicesInterop.Check(
            Native.cna_available_network_session_copy_session_properties(Handle, out CnaHandle properties), nameof(SessionProperties));
        try
        {
            SessionProperties = NetworkSessionProperties.CreateReadOnly(NetworkSessionProperties.ReadNative(properties));
        }
        finally
        {
            _ = Native.cna_network_session_properties_destroy(properties);
        }
    }

    internal CnaHandle Handle => new(_handle.DangerousGetHandle());

    internal AvailableNetworkSessionCollection Parent { get; }

    public string HostGamertag { get; }

    public int CurrentGamerCount { get; }

    public int OpenPublicGamerSlots { get; }

    public int OpenPrivateGamerSlots { get; }

    public NetworkSessionProperties SessionProperties { get; }

    public QualityOfService QualityOfService
    {
        get
        {
            if (!_qualityOfService.IsAvailable && !Parent.IsDisposed)
            {
                CnaQualityOfService value = GamerServicesInterop.Versioned<CnaQualityOfService>();
                GamerServicesInterop.Check(
                    Native.cna_available_network_session_get_quality_of_service(Handle, ref value), nameof(QualityOfService));
                _qualityOfService.Update(value);
            }

            return _qualityOfService;
        }
    }

    private delegate CnaResult IntQuery(CnaHandle session, out int value);

    private int Read(IntQuery query, string operation)
    {
        GamerServicesInterop.Check(query(Handle, out int value), operation);
        return value;
    }

    private static CnaResult CopyHostGamertag(CnaHandle handle, byte* destination, ulong capacity, out ulong bytes) =>
        Native.cna_available_network_session_copy_host_gamertag(handle, destination, capacity, out bytes);
}
