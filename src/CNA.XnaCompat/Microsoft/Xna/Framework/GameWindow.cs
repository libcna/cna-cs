namespace Microsoft.Xna.Framework;

/// <summary>
/// The system window associated with a <see cref="Game"/>.
///
/// CNA owns the platform-window lifetime, but that fact is intentionally private. XNA's public
/// contract is an abstract base class, so the native window is projected through the private
/// implementation created by <see cref="Game"/> instead of leaking <c>CNA.GameWindow</c> into a
/// game's base-type graph.
/// </summary>
public abstract class GameWindow
{
    private string _title = string.Empty;
    private bool _titleAssignedByGame;

    internal GameWindow()
    {
    }

    internal void Attach(CNA.GameWindow backend)
    {
        ArgumentNullException.ThrowIfNull(backend);
        _title = backend.Title;
        Backend = backend;
        lock (Attached)
        {
            Attached.RemoveAll(reference => !reference.TryGetTarget(out _));
            Attached.Add(new WeakReference<GameWindow>(this));
        }
    }

    /// <summary>The CNA window behind this one, for CNA.WindowsFormsCompat's form.</summary>
    internal CNA.GameWindow? Backend { get; private set; }

    // Under XNA every game window is a Windows Forms form, and a game reaches it with
    // Control.FromHandle(Window.Handle) (the Racing Game Kit hides its form while it loads).
    // CNA.WindowsFormsCompat answers that question through this list.
    private static readonly List<WeakReference<GameWindow>> Attached = [];

    /// <summary>The live game window whose <see cref="Handle"/> is <paramref name="handle"/>, or null.</summary>
    internal static GameWindow? FromHandle(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        lock (Attached)
        {
            foreach (WeakReference<GameWindow> reference in Attached)
            {
                if (reference.TryGetTarget(out GameWindow? window) && HandleOf(window) == handle)
                {
                    return window;
                }
            }
        }
        return null;
    }

    private static IntPtr HandleOf(GameWindow window)
    {
        try
        {
            return window.Handle;
        }
        catch (Exception ex) when (ex is ObjectDisposedException or CNA.CnaException)
        {
            // A disposed game's window, not yet collected: its native handle is gone.
            return IntPtr.Zero;
        }
    }

    public abstract bool AllowUserResizing { get; set; }

    public abstract Rectangle ClientBounds { get; }

    public abstract DisplayOrientation CurrentOrientation { get; }

    public abstract IntPtr Handle { get; }

    public abstract string ScreenDeviceName { get; }

    public string Title
    {
        get => _title;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (_title == value)
            {
                return;
            }

            SetTitle(value);
            _title = value;
            _titleAssignedByGame = true;
        }
    }

    /// <summary>
    /// XNA's own default window title, which CNA does not supply.
    ///
    /// Real XNA names the window before the game's <c>Initialize</c> runs
    /// (<c>WindowsGameWindow</c>'s constructor: <c>base.Title = GetDefaultTitleName()</c>), so a
    /// game that never touches <c>Window.Title</c> -- which is most of them -- still gets a titled
    /// window. Without this the window is untitled, which is not only wrong-looking: a blank title
    /// is what window managers, screenshot tools and <c>xdotool</c> match on, so the window becomes
    /// hard to address by name.
    ///
    /// The order is XNA's: the entry assembly's <see cref="System.Reflection.AssemblyTitleAttribute"/>
    /// when it is non-empty, then the executable's own file name, then the literal "Game". The
    /// middle step is the one that matters in practice -- .NET's SDK emits an
    /// <c>AssemblyTitleAttribute</c> holding the assembly name for every project that does not set
    /// one, so the first step usually answers, and the fallbacks exist for a host that publishes
    /// without it.
    ///
    /// Applied only when the game has not set a title itself. Assignment order is not something a
    /// game should have to think about: setting <c>Window.Title</c> in a constructor happens before
    /// the window exists, and it must not be undone by a default arriving later.
    /// </summary>
    internal void ApplyDefaultTitle()
    {
        if (_titleAssignedByGame)
        {
            return;
        }

        string title = DefaultTitleName();
        SetTitle(title);
        _title = title;
    }

    private static string DefaultTitleName()
    {
        System.Reflection.Assembly? entry = System.Reflection.Assembly.GetEntryAssembly();

        var attribute = entry?.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), true)
            is object[] { Length: > 0 } found
            ? found[0] as System.Reflection.AssemblyTitleAttribute
            : null;

        if (!string.IsNullOrEmpty(attribute?.Title))
        {
            return attribute!.Title;
        }

        string? location = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(location))
        {
            string name = Path.GetFileNameWithoutExtension(location);

            // A framework-dependent .NET app runs as "dotnet", which names the host rather than the
            // game. XNA never saw that case because its executable was the game.
            if (!string.IsNullOrEmpty(name) &&
                !string.Equals(name, "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
        }

        string? assemblyName = entry?.GetName().Name;
        return string.IsNullOrEmpty(assemblyName) ? "Game" : assemblyName;
    }

    public event EventHandler<EventArgs>? ClientSizeChanged;

    public event EventHandler<EventArgs>? OrientationChanged;

    public event EventHandler<EventArgs>? ScreenDeviceNameChanged;

    public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

    public void EndScreenDeviceChange(string screenDeviceName) =>
        EndScreenDeviceChange(screenDeviceName, ClientBounds.Width, ClientBounds.Height);

    public abstract void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight);

    protected void OnActivated()
    {
    }

    protected void OnClientSizeChanged() => ClientSizeChanged?.Invoke(this, EventArgs.Empty);

    protected void OnDeactivated()
    {
    }

    protected void OnOrientationChanged() => OrientationChanged?.Invoke(this, EventArgs.Empty);

    protected void OnPaint()
    {
    }

    protected void OnScreenDeviceNameChanged() => ScreenDeviceNameChanged?.Invoke(this, EventArgs.Empty);

    protected internal abstract void SetSupportedOrientations(DisplayOrientation orientations);

    protected abstract void SetTitle(string title);
}
