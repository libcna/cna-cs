using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's process-wide gamer-services pump, over CNA's native dispatcher. Initializing it installs
/// CNA's Guide into the running game: the game then presents the Guide and routes input to it
/// while it is visible, as an XNA game does.
/// </summary>
public static class GamerServicesDispatcher
{
    private static EventHandler<EventArgs>? s_installingTitleUpdate;
    private static CnaHandle s_installingTitleUpdateRegistration;

    public static bool IsInitialized
    {
        get
        {
            GamerServicesInterop.Check(
                Native.cna_gamer_services_dispatcher_get_is_initialized(out byte value), nameof(IsInitialized));
            return value != 0;
        }
    }

    public static IntPtr WindowHandle
    {
        get
        {
            GamerServicesInterop.Check(
                Native.cna_gamer_services_dispatcher_get_window_handle(out ulong value), nameof(WindowHandle));
            return unchecked((IntPtr)(long)value);
        }
        set => GamerServicesInterop.Check(
            Native.cna_gamer_services_dispatcher_set_window_handle(unchecked((ulong)(long)value)), nameof(WindowHandle));
    }

    /// <summary>
    /// XNA's order of checks (its IL): already initialized, then a null provider. CNA's dispatcher
    /// belongs to the running game, so the provider is checked and the game is the one CNA runs.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Gamer services are already initialized.");
        }

        ArgumentNullException.ThrowIfNull(serviceProvider);
        CNA.Game game = CNA.Game.Active ?? throw new InvalidOperationException(
            "CNA's gamer services run inside a game; create the Game before initializing them.");
        GamerServicesInterop.Check(
            Native.cna_gamer_services_dispatcher_initialize(new CnaHandle(game.NativeHandle)), nameof(Initialize));
    }

    public static void Update() =>
        GamerServicesInterop.Check(Native.cna_gamer_services_dispatcher_update(), nameof(Update));

    public static event EventHandler<EventArgs> InstallingTitleUpdate
    {
        add
        {
            if (s_installingTitleUpdate is null && value is not null)
            {
                Subscribe();
            }

            s_installingTitleUpdate += value;
        }
        remove
        {
            s_installingTitleUpdate -= value;
            if (s_installingTitleUpdate is null)
            {
                Unsubscribe();
            }
        }
    }

    private static unsafe void Subscribe()
    {
        GamerServicesInterop.Check(
            Native.cna_gamer_services_dispatcher_subscribe_installing_title_update_ext(
                (nint)(delegate* unmanaged[Cdecl]<nint, void>)&OnInstallingTitleUpdate, 0, out s_installingTitleUpdateRegistration),
            nameof(InstallingTitleUpdate));
    }

    private static void Unsubscribe()
    {
        if (s_installingTitleUpdateRegistration.IsNull)
        {
            return;
        }

        _ = Native.cna_gamer_unsubscribe_ext(s_installingTitleUpdateRegistration);
        s_installingTitleUpdateRegistration = CnaHandle.Zero;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnInstallingTitleUpdate(nint context)
    {
        try
        {
            s_installingTitleUpdate?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            GamerServicesInterop.ReportCallbackException(exception);
        }
    }
}
