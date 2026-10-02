namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// XNA's keyboard. XNA read the calling thread's key state, so it answered anywhere -- also in a
/// field initializer, which runs before the <c>Game</c> constructor (dsplaisted/Disentanglement keeps
/// <c>Keyboard.GetState()</c> as its previous state there); with no window yet, no key is down. CNA
/// reads keys through the game, so until one exists the state is that empty one (CSX-123).
/// </summary>
public static class Keyboard
{
    public static KeyboardState GetState() =>
        InputBeforeGame.HasGame ? new(CNA.Input.Keyboard.GetState()) : default;

    /// <summary>Matches real XNA's <c>GetState(PlayerIndex)</c>.</summary>
    public static KeyboardState GetState(PlayerIndex playerIndex) =>
        InputBeforeGame.HasGame ? new(CNA.Input.Keyboard.GetState((CNA.PlayerIndex)(int)playerIndex)) : default;
}

/// <summary>Whether a game exists for the static input classes to read through.</summary>
internal static class InputBeforeGame
{
    internal static bool HasGame => !CNA.Interop.CnaAmbientGame.Current.IsNull;
}
