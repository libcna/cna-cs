namespace Microsoft.Phone.Shell;

/// <summary>
/// The Windows Phone application lifetime service, as an XNA phone game sees it (cna-cs CSX-097).
///
/// On the phone the XAP host owned it: <see cref="Launching"/> on a fresh start, <see cref="Closing"/>
/// when the user backed out of the game, <see cref="Deactivated"/>/<see cref="Activated"/> around
/// tombstoning, with <see cref="State"/> surviving the tombstone. Here the game's host is a desktop
/// process: <see cref="Launching"/> is raised once the game is running -- after <c>Initialize</c>
/// and <c>LoadContent</c>, before the first <c>Update</c>, which is where NinjAcademy's handler
/// expects its audio to be set up -- and <see cref="Closing"/> when the game exits. Nothing
/// tombstones a desktop process, so <see cref="Deactivated"/> and <see cref="Activated"/> are never
/// raised and <see cref="State"/> lives as long as the process.
/// </summary>
public class PhoneApplicationService
{
    private static readonly Lazy<PhoneApplicationService> Instance = new(() => new PhoneApplicationService());

    public PhoneApplicationService()
    {
        Microsoft.Xna.Framework.Game.Started += OnGameStarted;
    }

    /// <summary>The service of the running application.</summary>
    public static PhoneApplicationService Current => Instance.Value;

    /// <summary>Transient state the application keeps across a deactivation.</summary>
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();

    public event EventHandler<LaunchingEventArgs>? Launching;

    public event EventHandler<ActivatedEventArgs>? Activated;

    public event EventHandler<DeactivatedEventArgs>? Deactivated;

    public event EventHandler<ClosingEventArgs>? Closing;

    private void OnGameStarted(Microsoft.Xna.Framework.Game game)
    {
        game.Exiting += (_, _) => Closing?.Invoke(this, new ClosingEventArgs());
        Launching?.Invoke(this, new LaunchingEventArgs());
    }

    // Kept so the never-raised events are not reported as unused; a host that can tombstone would
    // raise them here.
    internal void RaiseDeactivated() => Deactivated?.Invoke(this, new DeactivatedEventArgs());

    internal void RaiseActivated() => Activated?.Invoke(this, new ActivatedEventArgs());
}

/// <summary>Arguments of <see cref="PhoneApplicationService.Launching"/>.</summary>
public sealed class LaunchingEventArgs : EventArgs
{
}

/// <summary>Arguments of <see cref="PhoneApplicationService.Activated"/>.</summary>
public sealed class ActivatedEventArgs : EventArgs
{
}

/// <summary>Arguments of <see cref="PhoneApplicationService.Deactivated"/>.</summary>
public sealed class DeactivatedEventArgs : EventArgs
{
}

/// <summary>Arguments of <see cref="PhoneApplicationService.Closing"/>.</summary>
public sealed class ClosingEventArgs : EventArgs
{
}
