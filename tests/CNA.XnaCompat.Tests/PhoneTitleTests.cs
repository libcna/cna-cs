using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace CnaCs.XnaCompat.Tests;

/// <summary>
/// Player one of a Windows Phone title off a phone is the phone (cna-cs CSX-095): connected, as
/// <c>GamePad.GetState(PlayerIndex.One)</c> always is on Windows Phone, with Escape as its Back
/// button. UISample's main menu leaves on that button alone.
/// </summary>
public class PhoneTitleTests
{
    private static GamePadState WithPhoneBackButton(GamePadState state, bool escapeDown) =>
        (GamePadState)typeof(GraphicsDeviceManager).Assembly
            .GetType("Microsoft.Xna.Framework.PhoneTitle", throwOnError: true)!
            .GetMethod("WithPhoneBackButton", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [state, escapeDown])!;

    [Fact]
    public void WithoutAPad_ThePhoneIsAConnectedPadWithNothingHeld()
    {
        GamePadState state = WithPhoneBackButton(default, escapeDown: false);
        Assert.True(state.IsConnected);
        Assert.Equal(ButtonState.Released, state.Buttons.Back);
    }

    [Fact]
    public void WithoutAPad_EscapeIsTheBackButton()
    {
        GamePadState state = WithPhoneBackButton(default, escapeDown: true);
        Assert.True(state.IsConnected);
        Assert.True(state.IsButtonDown(Buttons.Back));
        Assert.False(state.IsButtonDown(Buttons.A));
    }

    [Fact]
    public void WithAPad_EscapeAddsBackAndKeepsTheRest()
    {
        var pad = new GamePadState(new Vector2(0.5f, 0f), Vector2.Zero, 0f, 1f, Buttons.A);
        GamePadState state = WithPhoneBackButton(pad, escapeDown: true);
        Assert.True(state.IsButtonDown(Buttons.Back));
        Assert.True(state.IsButtonDown(Buttons.A));
        Assert.Equal(new Vector2(0.5f, 0f), state.ThumbSticks.Left);
        Assert.Equal(1f, state.Triggers.Right);
    }

    [Fact]
    public void WithAPad_WithoutEscape_TheStateIsUnchanged()
    {
        var pad = new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, Buttons.B);
        Assert.Equal(pad, WithPhoneBackButton(pad, escapeDown: false));
    }

    private static readonly Type PhoneTitleType = typeof(GraphicsDeviceManager).Assembly
        .GetType("Microsoft.Xna.Framework.PhoneTitle", throwOnError: true)!;

    private static void SetFlag(string name, bool value) =>
        PhoneTitleType.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, value);

    private static bool GetFlag(string name) =>
        (bool)PhoneTitleType.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    private static int EnsureGuide(bool active, bool initializedByTheGame)
    {
        SetFlag("Active", active);
        SetFlag("PumpsGamerServices", false);
        int initializations = 0;
        bool initialized = initializedByTheGame;
        Func<bool> isInitialized = () => initialized;
        Action initialize = () => { initializations++; initialized = true; };
        try
        {
            MethodInfo ensure = PhoneTitleType.GetMethod("EnsureGuide", BindingFlags.NonPublic | BindingFlags.Static)!;
            ensure.Invoke(null, [isInitialized, initialize]);
            ensure.Invoke(null, [isInitialized, initialize]);
            return initializations;
        }
        finally
        {
            SetFlag("Active", false);
        }
    }

    /// <summary>
    /// Microsoft's Saving Embedded Images sample, a Windows Phone title, asks for a file name with
    /// Guide.BeginShowKeyboardInput and has no GamerServicesComponent: the phone's Guide needed none.
    /// A phone title's first Guide screen starts gamer services once, and the facade then pumps them
    /// (CSX-119).
    /// </summary>
    [Fact]
    public void APhoneTitlesGuide_StartsGamerServicesOnce_AndPumpsThem()
    {
        Assert.Equal(1, EnsureGuide(active: true, initializedByTheGame: false));
        SetFlag("Active", true);
        Assert.True(GetFlag("PumpsGamerServices"));
        SetFlag("Active", false);
        SetFlag("PumpsGamerServices", false);
    }

    [Fact]
    public void AWindowsTitle_OrAPhoneTitleWithItsOwnComponent_IsLeftAlone()
    {
        Assert.Equal(0, EnsureGuide(active: false, initializedByTheGame: false));
        Assert.Equal(0, EnsureGuide(active: true, initializedByTheGame: true));
        Assert.False(GetFlag("PumpsGamerServices"));
    }
}
