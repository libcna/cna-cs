using System.Collections.ObjectModel;
using CNA;
using CNA.Interop;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// The sessions a search found. Each element is copied out of native's collection when the search
/// ends, so it outlives the collection's own handle; disposing the collection is what makes its
/// sessions unjoinable, as in XNA.
/// </summary>
public sealed class AvailableNetworkSessionCollection : ReadOnlyCollection<AvailableNetworkSession>, IDisposable
{
    private readonly NativeResourceHandle _handle;

    private AvailableNetworkSessionCollection(CnaHandle handle, List<AvailableNetworkSession> sessions)
        : base(sessions)
    {
        _handle = new NativeResourceHandle(
            handle.Value, value => Native.cna_available_network_session_collection_destroy(new CnaHandle(value)).IsSuccess());
    }

    ~AvailableNetworkSessionCollection()
    {
        Dispose(false);
    }

    private bool _isDisposed;

    public bool IsDisposed => _isDisposed;

    internal static AvailableNetworkSessionCollection FromNative(CnaHandle handle)
    {
        var sessions = new List<AvailableNetworkSession>();
        var collection = new AvailableNetworkSessionCollection(handle, sessions);
        GamerServicesInterop.Check(
            Native.cna_available_network_session_collection_get_count(handle, out int count), nameof(AvailableNetworkSessionCollection));
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(
                Native.cna_available_network_session_collection_copy_session(handle, index, out CnaHandle session),
                nameof(AvailableNetworkSessionCollection));
            sessions.Add(new AvailableNetworkSession(session, collection));
        }

        return collection;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (disposing)
        {
            _handle.Dispose();
        }
    }
}
