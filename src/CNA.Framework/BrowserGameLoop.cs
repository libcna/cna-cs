using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace CNA;

/// <summary>
/// <see cref="Game.Run"/> in a browser. A page cannot block, so the run goes on in the browser's
/// animation frames: each one runs one frame of a host-driven native run
/// (<c>cna_game_run_frame_ext</c>), and the frame that finds the game exited ends the run --
/// delivering <c>Exiting</c> and <c>EndRun</c> as <see cref="Game.Run"/> does -- and calls the
/// ending action, which is where a <c>Dispose</c> requested while the run was live happens. The
/// frames are requested from here, so a page needs nothing beyond <c>runMain()</c>.
/// </summary>
[SupportedOSPlatform("browser")]
internal static partial class BrowserGameLoop
{
    private static readonly Action<double> Frame = OnAnimationFrame;
    private static Game? _game;
    private static Action? _ended;

    internal static bool IsRunning(Game game) => ReferenceEquals(_game, game);

    internal static void Start(Game game, Action ended)
    {
        if (_game is not null)
        {
            throw new InvalidOperationException("Another game is already running in this page.");
        }

        _game = game;
        _ended = ended;
        RequestAnimationFrame(Frame);
    }

    [JSImport("globalThis.requestAnimationFrame")]
    private static partial int RequestAnimationFrame(
        [JSMarshalAs<JSType.Function<JSType.Number>>] Action<double> callback);

    /// <summary>A frame that throws -- a game callback did -- ends the run before the exception
    /// reaches the page, which reports it; the game does not go on drawing after it.</summary>
    private static void OnAnimationFrame(double timestamp)
    {
        _ = timestamp;
        Game game = _game!;
        bool running = false;
        try
        {
            running = game.RunFrame();
        }
        finally
        {
            if (running)
            {
                RequestAnimationFrame(Frame);
            }
            else
            {
                Action ended = _ended!;
                _game = null;
                _ended = null;
                ended();
            }
        }
    }
}
