namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's gamer-services component, as its IL has it: initialization points the dispatcher at the
/// game's window and services and exits the game when a title update installs; every update pumps
/// the dispatcher.
/// </summary>
public class GamerServicesComponent : GameComponent
{
    public GamerServicesComponent(Game game)
        : base(game)
    {
    }

    public override void Initialize()
    {
        GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
        GamerServicesDispatcher.InstallingTitleUpdate += OnInstallingTitleUpdate;
        GamerServicesDispatcher.Initialize(Game.Services);
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        GamerServicesDispatcher.Update();
        base.Update(gameTime);
    }

    private void OnInstallingTitleUpdate(object? sender, EventArgs e) => Game.Exit();
}
