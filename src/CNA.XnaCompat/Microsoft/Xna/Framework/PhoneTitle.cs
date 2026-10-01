namespace Microsoft.Xna.Framework;

/// <summary>
/// A Windows Phone title running off a phone -- on a desktop, as in the phone emulator's window.
/// XNA's build marks the title in its <c>Microsoft.Xna.Framework.RuntimeProfile</c> resource
/// (<c>WindowsPhone.v4.0.&lt;profile&gt;</c>). Two things the phone itself supplied have no desktop
/// counterpart, and the facade stands in for them: full screen is the status bar, not a display
/// mode (cna-cs CSX-094), and player one's pad is the phone, with its Back button (CSX-095).
/// On Android and iOS the host is a phone and none of this applies.
/// </summary>
internal static class PhoneTitle
{
    /// <summary>Set by each <see cref="Game"/> from its own assembly.</summary>
    internal static bool Active { get; set; }

    internal static bool IsOffAPhone(string? runtimeProfileLine) =>
        IsOffAPhone(runtimeProfileLine, OperatingSystem.IsAndroid() || OperatingSystem.IsIOS());

    internal static bool IsOffAPhone(string? runtimeProfileLine, bool hostIsPhone) =>
        !hostIsPhone && runtimeProfileLine is not null &&
        runtimeProfileLine.StartsWith("WindowsPhone.", StringComparison.Ordinal);

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
