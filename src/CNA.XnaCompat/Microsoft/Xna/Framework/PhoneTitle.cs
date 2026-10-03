namespace Microsoft.Xna.Framework;

/// <summary>
/// A Windows Phone title running off a phone -- on a desktop, as in the phone emulator's window.
/// XNA's build marks the title in its <c>Microsoft.Xna.Framework.RuntimeProfile</c> resource
/// (<c>WindowsPhone.v4.0.&lt;profile&gt;</c>). Two things the phone itself supplied have no desktop
/// counterpart, and the facade stands in for them: full screen is the status bar, not a display
/// mode (cna-cs CSX-094), player one's pad is the phone, with its Back button (CSX-095), the
/// mouse is the finger, as in the emulator (CSX-098), and the Guide's keyboard prompt and message box
/// need no GamerServicesComponent (CSX-119), nor does its trial mode (CSX-137).
/// On Android and iOS the host is a phone and none of this applies.
/// </summary>
internal static class PhoneTitle
{
    /// <summary>Set by each <see cref="Game"/> from its own assembly.</summary>
    internal static bool Active { get; set; }

    /// <summary>The phone title running now, whose services gamer services start with.</summary>
    internal static Game? Title { get; set; }

    /// <summary>Whether the facade started gamer services for the Guide, and so pumps them each
    /// frame, as a GamerServicesComponent would.</summary>
    internal static bool PumpsGamerServices { get; set; }

    /// <summary>
    /// On Windows Phone the Guide's keyboard prompt and message box were the phone's own and needed no
    /// GamerServicesComponent: Microsoft's Saving Embedded Images sample asks for a file name with
    /// <c>Guide.BeginShowKeyboardInput</c> and has none. On Windows, XNA's Guide needs the dispatcher
    /// initialized (its IL dispatches through <c>GamerServicesDispatcher.PacketBuffer</c>). So a phone
    /// title starts gamer services the first time the Guide shows itself, unless its own component
    /// already did; a Windows title is left to fail as on Windows.
    /// </summary>
    internal static void EnsureGuide(Func<bool> isInitialized, Action initialize)
    {
        if (!Active || PumpsGamerServices || isInitialized())
        {
            return;
        }

        initialize();
        PumpsGamerServices = true;
    }

    internal static bool IsOffAPhone(string? runtimeProfileLine) =>
        IsOffAPhone(runtimeProfileLine, OperatingSystem.IsAndroid() || OperatingSystem.IsIOS());

    internal static bool IsOffAPhone(string? runtimeProfileLine, bool hostIsPhone) =>
        !hostIsPhone && runtimeProfileLine is not null &&
        runtimeProfileLine.StartsWith("WindowsPhone.", StringComparison.Ordinal);

    /// <summary>Called once the game runs: the mouse reaches <c>TouchPanel</c> as a finger, as it
    /// did in the phone emulator. A Windows title keeps XNA's default, touch from a digitizer only.</summary>
    internal static void OnStarted() => CNA.Input.Touch.TouchPanel.MouseTouchEmulationEnabled = true;

    /// <summary>
    /// Player one's state as a phone title sees it: the phone is always a connected pad
    /// (<c>GamePad.GetState(PlayerIndex.One).IsConnected</c> is true on Windows Phone), and its
    /// hardware Back button is the desktop's Escape key -- the substitute the owner chose for the
    /// C++ ports of these samples, here without touching the game. A real pad's state is kept and
    /// Back added to it.
    /// </summary>
    internal static Input.GamePadState WithPhoneBackButton(Input.GamePadState state, bool escapeDown)
    {
        if (!escapeDown)
        {
            return state.IsConnected ? state : new Input.GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f);
        }

        Input.GamePadButtons held = state.Buttons;
        Input.Buttons buttons = Input.Buttons.Back
            | Flag(held.A, Input.Buttons.A) | Flag(held.B, Input.Buttons.B)
            | Flag(held.X, Input.Buttons.X) | Flag(held.Y, Input.Buttons.Y)
            | Flag(held.Start, Input.Buttons.Start) | Flag(held.BigButton, Input.Buttons.BigButton)
            | Flag(held.LeftShoulder, Input.Buttons.LeftShoulder) | Flag(held.RightShoulder, Input.Buttons.RightShoulder)
            | Flag(held.LeftStick, Input.Buttons.LeftStick) | Flag(held.RightStick, Input.Buttons.RightStick);
        return new Input.GamePadState(state.ThumbSticks, state.Triggers, new Input.GamePadButtons(buttons), state.DPad);
    }

    private static Input.Buttons Flag(Input.ButtonState state, Input.Buttons button) =>
        state == Input.ButtonState.Pressed ? button : 0;
}
