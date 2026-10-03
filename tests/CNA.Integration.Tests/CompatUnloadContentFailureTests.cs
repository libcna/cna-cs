using Xunit;

namespace CNA.Integration.Tests;

/// <summary>
/// XNA's <c>Game.Dispose</c> unloads content by disposing the graphics device, whose
/// <c>Disposing</c> event reaches <c>Game.DeviceDisposing</c> and so <c>UnloadContent</c>; nothing on
/// that path catches (XNA IL: <c>Game::Dispose(bool)</c> is a try/finally around its lock,
/// <c>GraphicsDeviceManager::Dispose</c> and <c>GraphicsDevice::~GraphicsDevice</c> have no handler).
/// An exception <c>UnloadContent</c> throws therefore leaves <c>Dispose</c>, and the
/// <c>using (var game = new Game1()) game.Run();</c> of every template with it (CSX-142).
///
/// spectrumbranch/gearsvge's GearsDebug is the case: its <c>UnloadContent</c> stops its audio thread
/// with <c>Thread.Abort</c>, which .NET 5 and later refuse with
/// <see cref="PlatformNotSupportedException"/>. The binding swallowed it, so the foreground thread
/// kept the process alive with no window and no message.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class CompatUnloadContentFailureTests
{
    private sealed class ThrowingUnloadGame : Microsoft.Xna.Framework.Game
    {
        public ThrowingUnloadGame()
        {
            _ = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
        }

        public int UnloadContentCalls { get; private set; }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Exit();
            base.Update(gameTime);
        }

        protected override void UnloadContent()
        {
            UnloadContentCalls++;
            throw new PlatformNotSupportedException("Thread abort is not supported on this platform.");
        }
    }

    private sealed class ExitingGame : Microsoft.Xna.Framework.Game
    {
        public ExitingGame()
        {
            _ = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Exit();
            base.Update(gameTime);
        }
    }

    [NativeFact]
    public void AnExceptionUnloadContentThrows_LeavesDispose()
    {
        var game = new ThrowingUnloadGame();
        game.Run();

        PlatformNotSupportedException thrown = Assert.Throws<PlatformNotSupportedException>(game.Dispose);

        Assert.Equal("Thread abort is not supported on this platform.", thrown.Message);
        Assert.Equal(1, game.UnloadContentCalls);
        game.Dispose();
        Assert.Equal(1, game.UnloadContentCalls);
    }

    [NativeFact]
    public void AGameWhoseUnloadContentThrew_IsStillReleased()
    {
        global::CNA.GameDestroyMetrics before = global::CNA.Game.GetDestroyMetrics();
        var game = new ThrowingUnloadGame();
        game.Run();
        Assert.Throws<PlatformNotSupportedException>(game.Dispose);

        // The native game was destroyed before the exception left Dispose, so the next one creates
        // and runs; the failure is not booked as a refused destroy either.
        using var next = new ExitingGame();
        next.Run();
        Assert.Equal(0, (global::CNA.Game.GetDestroyMetrics() - before).RefusedDestroys);
    }
}
