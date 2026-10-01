using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace CNA.Integration.Tests;

/// <summary>
/// SIGTERM during <c>Run</c> ends the game the way closing its window does: <c>Run</c> returns and
/// the process carries on. The runtime's default -- <c>exit()</c> on a signal thread while the game
/// thread is inside GL and X -- deadlocked a sample capture in libGLX's destructor and crashed the
/// template on every renderer but one (`docs/native-behavior-blockers.md`). Without the fix this
/// test does not fail politely: the signal ends the test host.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class TerminationSignalTests(ITestOutputHelper output)
{
    private const int SignalFrame = 3;
    private const int GiveUpFrame = 600;

    [DllImport("libc", EntryPoint = "kill")]
    private static extern int Kill(int pid, int signal);

    private sealed class SignalledGame : Microsoft.Xna.Framework.Game
    {
        public SignalledGame()
        {
            _ = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
        }

        public int Updates { get; private set; }

        public bool ExitedOnItsOwn { get; private set; }

        public bool RaisedExiting { get; private set; }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Updates++;
            if (Updates == SignalFrame)
            {
                Assert.Equal(0, Kill(Environment.ProcessId, 15));
            }
            else if (Updates == GiveUpFrame)
            {
                ExitedOnItsOwn = true;
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            RaisedExiting = true;
            base.OnExiting(sender, args);
        }
    }

    [NativeFact]
    public void Sigterm_EndsRunThroughTheGamesOwnExit()
    {
        using var game = new SignalledGame();
        game.Run();

        output.WriteLine($"updates {game.Updates}");
        Assert.False(game.ExitedOnItsOwn, "the signal never reached the game loop");
        Assert.True(game.RaisedExiting);
        Assert.InRange(game.Updates, SignalFrame, GiveUpFrame - 1);
    }
}
