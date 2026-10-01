using System.Collections.ObjectModel;
using System.Text;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

public sealed unsafe class LeaderboardReader : IDisposable
{
    private static readonly object s_readOwner = new();
    private readonly NativeResourceHandle _handle;
    private readonly object _pageOwner = new();
    private ReadOnlyCollection<LeaderboardEntry> _entries = null!;

    private LeaderboardReader(CnaHandle handle)
    {
        _handle = new NativeResourceHandle(
            handle.Value, value => Native.cna_leaderboard_reader_destroy(new CnaHandle(value)).IsSuccess());
        LoadEntries();
    }

    private CnaHandle Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle.IsClosed, this);
            return new CnaHandle(_handle.DangerousGetHandle());
        }
    }

    public LeaderboardIdentity LeaderboardIdentity
    {
        get
        {
            CnaLeaderboardIdentity identity = GamerServicesInterop.Versioned<CnaLeaderboardIdentity>();
            GamerServicesInterop.Check(Native.cna_leaderboard_reader_get_identity(Handle, ref identity), nameof(LeaderboardIdentity));
            return FromNative(identity);
        }
    }

    public int TotalLeaderboardSize => Info().TotalLeaderboardSize;

    public int PageStart => Info().PageStart;

    public ReadOnlyCollection<LeaderboardEntry> Entries => _entries;

    public bool IsDisposed => _handle.IsClosed;

    public bool CanPageUp => Info().CanPageUp != 0;

    public bool CanPageDown => Info().CanPageDown != 0;

    public static LeaderboardReader Read(LeaderboardIdentity leaderboardId, IEnumerable<Gamer> gamers, Gamer pivotGamer, int pageSize) =>
        new(ReadFromGamers(leaderboardId, gamers, pivotGamer, pageSize));

    public static LeaderboardReader Read(LeaderboardIdentity leaderboardId, Gamer pivotGamer, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(pivotGamer);
        CnaLeaderboardIdentity identity = ToNative(leaderboardId);
        GamerServicesInterop.Check(
            Native.cna_leaderboard_reader_read_from_pivot(&identity, pivotGamer.Handle, pageSize, out CnaHandle reader), nameof(Read));
        return new(reader);
    }

    public static LeaderboardReader Read(LeaderboardIdentity leaderboardId, int pageStart, int pageSize)
    {
        CnaLeaderboardIdentity identity = ToNative(leaderboardId);
        GamerServicesInterop.Check(
            Native.cna_leaderboard_reader_read(&identity, pageStart, pageSize, out CnaHandle reader), nameof(Read));
        return new(reader);
    }

    public static IAsyncResult BeginRead(
        LeaderboardIdentity leaderboardId, IEnumerable<Gamer> gamers, Gamer pivotGamer, int pageSize, AsyncCallback callback, object asyncState) =>
        Completed(Read(leaderboardId, gamers, pivotGamer, pageSize), callback, asyncState);

    public static IAsyncResult BeginRead(
        LeaderboardIdentity leaderboardId, Gamer pivotGamer, int pageSize, AsyncCallback callback, object asyncState) =>
        Completed(Read(leaderboardId, pivotGamer, pageSize), callback, asyncState);

    public static IAsyncResult BeginRead(
        LeaderboardIdentity leaderboardId, int pageStart, int pageSize, AsyncCallback callback, object asyncState) =>
        Completed(Read(leaderboardId, pageStart, pageSize), callback, asyncState);

    public static LeaderboardReader EndRead(IAsyncResult result) =>
        (LeaderboardReader)GamerServicesAsyncResult.ForEnd(result, s_readOwner).Payload!;

    public void PageUp()
    {
        GamerServicesInterop.Check(Native.cna_leaderboard_reader_page_up(Handle), nameof(PageUp));
        LoadEntries();
    }

    public void PageDown()
    {
        GamerServicesInterop.Check(Native.cna_leaderboard_reader_page_down(Handle), nameof(PageDown));
        LoadEntries();
    }

    public IAsyncResult BeginPageUp(AsyncCallback callback, object asyncState)
    {
        PageUp();
        return CompletedPage(callback, asyncState);
    }

    public IAsyncResult BeginPageDown(AsyncCallback callback, object asyncState)
    {
        PageDown();
        return CompletedPage(callback, asyncState);
    }

    public void EndPageUp(IAsyncResult result) => GamerServicesAsyncResult.ForEnd(result, _pageOwner);

    public void EndPageDown(IAsyncResult result) => GamerServicesAsyncResult.ForEnd(result, _pageOwner);

    public void Dispose() => _handle.Dispose();

    internal static unsafe CnaLeaderboardIdentity ToNative(LeaderboardIdentity identity)
    {
        CnaLeaderboardIdentity native = GamerServicesInterop.Versioned<CnaLeaderboardIdentity>();
        native.GameMode = identity.GameMode;
        byte[] key = Encoding.UTF8.GetBytes(identity.Key ?? string.Empty);
        if (key.Length >= 64)
        {
            throw new ArgumentException("The leaderboard key is longer than 63 UTF-8 bytes.", nameof(identity));
        }

        Span<byte> destination = native.Key;
        key.CopyTo(destination);
        return native;
    }

    private static LeaderboardIdentity FromNative(CnaLeaderboardIdentity native)
    {
        ReadOnlySpan<byte> key = native.Key;
        int length = key.IndexOf((byte)0);
        return new LeaderboardIdentity
        {
            Key = Encoding.UTF8.GetString(length < 0 ? key : key[..length]),
            GameMode = native.GameMode,
        };
    }

    private static unsafe CnaHandle ReadFromGamers(LeaderboardIdentity leaderboardId, IEnumerable<Gamer> gamers, Gamer pivotGamer, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(gamers);
        CnaHandle[] handles = gamers.Select(gamer => gamer.Handle).ToArray();
        CnaLeaderboardIdentity identity = ToNative(leaderboardId);
        CnaHandle pivot = pivotGamer?.Handle ?? CnaHandle.Zero;
        fixed (CnaHandle* list = handles)
        {
            GamerServicesInterop.Check(
                Native.cna_leaderboard_reader_read_from_gamers(
                    &identity, list, (ulong)handles.Length, pivot, pageSize, out CnaHandle reader),
                nameof(Read));
            return reader;
        }
    }

    private static IAsyncResult Completed(LeaderboardReader reader, AsyncCallback callback, object asyncState)
    {
        var result = new GamerServicesAsyncResult(callback, asyncState, s_readOwner) { Payload = reader };
        result.Complete(synchronously: true);
        return result;
    }

    private IAsyncResult CompletedPage(AsyncCallback callback, object asyncState)
    {
        var result = new GamerServicesAsyncResult(callback, asyncState, _pageOwner);
        result.Complete(synchronously: true);
        return result;
    }

    private CnaLeaderboardReaderInfo Info()
    {
        CnaLeaderboardReaderInfo info = GamerServicesInterop.Versioned<CnaLeaderboardReaderInfo>();
        GamerServicesInterop.Check(Native.cna_leaderboard_reader_get_info(Handle, ref info), nameof(LeaderboardReader));
        return info;
    }

    private void LoadEntries()
    {
        int count = Info().EntryCount;
        var entries = new List<LeaderboardEntry>(count);
        for (int index = 0; index < count; index++)
        {
            GamerServicesInterop.Check(Native.cna_leaderboard_reader_get_entry_at(Handle, index, out CnaHandle entry), nameof(Entries));
            entries.Add(new LeaderboardEntry(entry));
        }

        _entries = new ReadOnlyCollection<LeaderboardEntry>(entries);
    }
}
