using CNA.Interop;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// A machine in a network session: one object per machine, shared by its gamers, as XNA's is. Native
/// hands out a machine only as a copy taken through one of its gamers, so this object keeps a gamer
/// to take the copy through and reads the machine's gamers from a fresh copy each time.
/// </summary>
public sealed class NetworkMachine
{
    private readonly NetworkSession _session;
    private readonly List<NetworkGamer> _gamers = new();
    private readonly GamerCollection<NetworkGamer> _collection;
    private NetworkGamer _anchor;

    internal NetworkMachine(NetworkSession session, NetworkGamer anchor)
    {
        _session = session;
        _anchor = anchor;
        _collection = new GamerCollection<NetworkGamer>(_gamers);
    }

    public GamerCollection<NetworkGamer> Gamers
    {
        get
        {
            Refresh();
            return _collection;
        }
    }

    public void RemoveFromSession() =>
        WithCopy(machine => GamerServicesInterop.Check(
            Native.cna_network_machine_remove_from_session(machine), nameof(RemoveFromSession)));

    /// <summary>Re-reads this machine's gamers, and moves the anchor off a gamer that has left.</summary>
    internal void Refresh()
    {
        WithCopy(machine =>
        {
            GamerServicesInterop.Check(Native.cna_network_machine_get_gamer_count(machine, out int count), nameof(Gamers));
            _gamers.Clear();
            for (int index = 0; index < count; index++)
            {
                GamerServicesInterop.Check(Native.cna_network_machine_get_gamer(machine, index, out CnaHandle view), nameof(Gamers));
                try
                {
                    if (_session.Resolve(view) is { } gamer)
                    {
                        _gamers.Add(gamer);
                    }
                }
                finally
                {
                    GamerServicesInterop.Check(Native.cna_network_gamer_destroy(view), nameof(Gamers));
                }
            }
        });

        if (_gamers.Count > 0 && !_gamers.Contains(_anchor))
        {
            _anchor = _gamers[0];
        }
    }

    private void WithCopy(Action<CnaHandle> body)
    {
        GamerServicesInterop.Check(Native.cna_network_gamer_copy_machine(_anchor.Handle, out CnaHandle machine), nameof(NetworkMachine));
        try
        {
            body(machine);
        }
        finally
        {
            GamerServicesInterop.Check(Native.cna_network_machine_destroy(machine), nameof(NetworkMachine));
        }
    }
}
