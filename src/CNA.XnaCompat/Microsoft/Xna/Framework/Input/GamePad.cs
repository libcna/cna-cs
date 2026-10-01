namespace Microsoft.Xna.Framework.Input;

public static class GamePad
{
    public static GamePadState GetState(PlayerIndex playerIndex) =>
        AsThePhone(playerIndex, new(CNA.Input.GamePad.GetState((CNA.PlayerIndex)(int)playerIndex)));

    /// <summary>Matches real XNA's <c>GetState(PlayerIndex, GamePadDeadZone)</c>.</summary>
    public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode) =>
        AsThePhone(playerIndex, new(CNA.Input.GamePad.GetState(
            (CNA.PlayerIndex)(int)playerIndex, (CNA.Input.GamePadDeadZone)(int)deadZoneMode)));

    // Player one of a Windows Phone title off a phone is the phone (PhoneTitle, cna-cs CSX-095).
    private static GamePadState AsThePhone(PlayerIndex playerIndex, GamePadState state) =>
        PhoneTitle.Active && playerIndex == PlayerIndex.One
            ? PhoneTitle.WithPhoneBackButton(state, CNA.Input.Keyboard.GetState().IsKeyDown(CNA.Input.Keys.Escape))
            : state;

    /// <summary>Matches real XNA's <c>SetVibration</c>. <see langword="false"/> means the controller
    /// did not accept it, not that the call failed -- see
    /// <see cref="CNA.Input.GamePad.SetVibration"/>.</summary>
    public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor) =>
        CNA.Input.GamePad.SetVibration((CNA.PlayerIndex)(int)playerIndex, leftMotor, rightMotor);

    public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex) =>
        new(CNA.Input.GamePad.GetCapabilities((CNA.PlayerIndex)(int)playerIndex));
}
