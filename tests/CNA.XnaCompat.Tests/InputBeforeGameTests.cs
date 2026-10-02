using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace CNA.XnaCompat.Tests;

/// <summary>
/// XNA read input from the calling thread, so its static input classes answered before any game
/// existed -- a field initializer runs before the Game constructor (dsplaisted/Disentanglement keeps
/// its previous keyboard state there). With no window, nothing is pressed (CSX-123).
/// </summary>
public class InputBeforeGameTests
{
    [Fact]
    public void BeforeAGameExists_NothingIsPressed()
    {
        KeyboardState keyboard = Keyboard.GetState();
        Assert.Empty(keyboard.GetPressedKeys());
        Assert.True(keyboard.IsKeyUp(Keys.Space));
        Assert.Empty(Keyboard.GetState(Microsoft.Xna.Framework.PlayerIndex.One).GetPressedKeys());

        MouseState mouse = Mouse.GetState();
        Assert.Equal(ButtonState.Released, mouse.LeftButton);
        Assert.Equal(0, mouse.ScrollWheelValue);

        Assert.False(GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One).IsConnected);
        Assert.False(GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.Two, GamePadDeadZone.Circular).IsConnected);
    }
}
