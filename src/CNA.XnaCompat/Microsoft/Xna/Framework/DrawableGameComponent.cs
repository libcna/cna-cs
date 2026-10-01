namespace Microsoft.Xna.Framework;

/// <summary>XNA 4.0-compatible drawable component with the required
/// <c>DrawableGameComponent : GameComponent</c> public relationship.</summary>
public class DrawableGameComponent : GameComponent, IDrawable
{
    public DrawableGameComponent(Game game)
        : base(game, drawable: true)
    {
        DrawableInner.VisibleChanged += (_, eventArgs) => OnVisibleChanged(this, eventArgs);
        DrawableInner.DrawOrderChanged += (_, eventArgs) => OnDrawOrderChanged(this, eventArgs);
    }

    private CNA.DrawableGameComponent DrawableInner => (CNA.DrawableGameComponent)Inner;

    public Graphics.GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

    public bool Visible
    {
        get => DrawableInner.Visible;
        set => DrawableInner.Visible = value;
    }

    public int DrawOrder
    {
        get => DrawableInner.DrawOrder;
        set => DrawableInner.DrawOrder = value;
    }

    public event EventHandler<EventArgs>? VisibleChanged;

    public event EventHandler<EventArgs>? DrawOrderChanged;

    private bool _initialized;
    private bool _nativeLoadContentAlreadyDone;

    /// <summary>
    /// XNA's <c>DrawableGameComponent.Initialize</c> loads the component's content on its first
    /// call, inside the call, when a device exists (XNA IL): an override that reads what
    /// <see cref="LoadContent"/> set right after <c>base.Initialize()</c> sees it. CNA's own
    /// component loads its content when its native initialization follows; that first native
    /// <c>LoadContent</c> is the one already done here and is skipped once.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        if (!_initialized &&
            Game.Services.GetService(typeof(Graphics.IGraphicsDeviceService)) is Graphics.IGraphicsDeviceService
            {
                GraphicsDevice: not null,
            })
        {
            LoadContent();
            _nativeLoadContentAlreadyDone = true;
        }

        _initialized = true;
    }

    public virtual void Draw(GameTime gameTime)
    {
    }

    protected virtual void LoadContent()
    {
    }

    protected virtual void UnloadContent()
    {
    }

    protected override void Dispose(bool disposing) => base.Dispose(disposing);

    internal void InvokeLoadContent()
    {
        if (_nativeLoadContentAlreadyDone)
        {
            _nativeLoadContentAlreadyDone = false;
            return;
        }

        LoadContent();
    }

    internal void InvokeUnloadContent() => UnloadContent();

    protected virtual void OnDrawOrderChanged(object sender, EventArgs args) =>
        DrawOrderChanged?.Invoke(this, args);

    protected virtual void OnVisibleChanged(object sender, EventArgs args) =>
        VisibleChanged?.Invoke(this, args);
}
