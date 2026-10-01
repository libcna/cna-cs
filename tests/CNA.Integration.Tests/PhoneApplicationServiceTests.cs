using Microsoft.Phone.Shell;
using Xunit;
using XnaGame = Microsoft.Xna.Framework.Game;
using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaGraphicsDeviceManager = Microsoft.Xna.Framework.GraphicsDeviceManager;

namespace CNA.Integration.Tests;

/// <summary>
/// A Windows Phone game's lifetime events off the phone (cna-cs CSX-097): Launching once the game
/// runs -- after Initialize and LoadContent, before the first Update, where NinjAcademy's handler
/// needs the audio it set up in Initialize -- and Closing when it exits.
/// </summary>
[Collection(OwnGameCollection.Name)]
public class PhoneApplicationServiceTests
{
    private sealed class PhoneGame : XnaGame
    {
        private int _frames;

        public PhoneGame()
        {
            _ = new XnaGraphicsDeviceManager(this);
            PhoneApplicationService.Current.Launching += OnLaunching;
            PhoneApplicationService.Current.Closing += OnClosing;
        }

        public List<string> Order { get; } = [];

        private void OnLaunching(object? sender, LaunchingEventArgs e) => Order.Add("Launching");

        private void OnClosing(object? sender, ClosingEventArgs e) => Order.Add("Closing");

        public void Detach()
        {
            PhoneApplicationService.Current.Launching -= OnLaunching;
            PhoneApplicationService.Current.Closing -= OnClosing;
        }

        protected override void Initialize()
        {
            Order.Add("Initialize");
            base.Initialize();
        }

        protected override void LoadContent() => Order.Add("LoadContent");

        protected override void Update(XnaGameTime gameTime)
        {
            Order.Add("Update");
            if (++_frames == 2)
            {
                Exit();
            }

            base.Update(gameTime);
        }
    }

    [NativeFact]
    public void Launching_ArrivesBeforeTheFirstUpdate_AndClosingAtExit()
    {
        var game = new PhoneGame();
        try
        {
            game.Run();
            Assert.Equal(["Initialize", "LoadContent", "Launching", "Update"], game.Order.Take(4));
            Assert.Equal("Closing", game.Order[^1]);
            Assert.Single(game.Order, "Launching");
            Assert.Single(game.Order, "Closing");
        }
        finally
        {
            game.Detach();
            game.Dispose();
        }
    }

    [NativeFact]
    public void State_IsOneDictionaryForTheProcess()
    {
        PhoneApplicationService.Current.State["key"] = 42;
        try
        {
            Assert.Same(PhoneApplicationService.Current, PhoneApplicationService.Current);
            Assert.Equal(42, PhoneApplicationService.Current.State["key"]);
        }
        finally
        {
            PhoneApplicationService.Current.State.Remove("key");
        }
    }
}
