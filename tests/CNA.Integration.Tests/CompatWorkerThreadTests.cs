using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Xunit;
using XnaGame = Microsoft.Xna.Framework.Game;

// NOT under CNA, for the reason CompatLayerIntegrationTests records.
namespace CnaCs.Integration.Tests.Compat;

/// <summary>
/// XNA 4.0 let a game create graphics resources, move their data and load content on a thread of
/// its own while the game thread kept drawing -- the loading-screen pattern resonance-game's
/// level load uses. CNA's handles belong to the game thread, so the facade runs whole operations
/// there (cna-cs CSX-100) and CNA runs the remaining calls there (C ABI 0.39.0, CSX-101); before,
/// each one failed with CNA_RESULT_THREAD.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class CompatWorkerThreadTests
{
    private static string LzxRoot => Path.Combine(AppContext.BaseDirectory, "assets", "xnb", "lzx");

    private sealed class LoadingGame : XnaGame
    {
        private Thread? _worker;
        private int _frames;

        public LoadingGame()
        {
            // HiDef: the worker creates a volume texture.
            _ = new GraphicsDeviceManager(this) { GraphicsProfile = GraphicsProfile.HiDef };
            Content.RootDirectory = LzxRoot;
        }

        public Exception? WorkerFailure { get; private set; }

        public bool WorkerFinished { get; private set; }

        public int GameThreadId { get; private set; }

        public int WorkerThreadId { get; private set; }

        protected override void Update(GameTime gameTime)
        {
            if (_worker is null)
            {
                GameThreadId = Environment.CurrentManagedThreadId;
                _worker = new Thread(Load) { IsBackground = true };
                _worker.Start();
            }

            if (WorkerFinished || ++_frames > 1200)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        private void Load()
        {
            try
            {
                WorkerThreadId = Environment.CurrentManagedThreadId;
                LoadEverything(GraphicsDevice, Content);
            }
            catch (Exception ex)
            {
                WorkerFailure = ex;
            }

            WorkerFinished = true;
        }
    }

    private static void LoadEverything(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content)
    {
        Color[] pixels = Enumerable.Range(0, 16).Select(i => new Color(i * 16, 255 - i * 16, 7, 255)).ToArray();

        using var texture = new Texture2D(device, 4, 4);
        texture.SetData(pixels);
        var readBack = new Color[16];
        texture.GetData(readBack);
        Assert.Equal(pixels, readBack);
        Assert.Equal(4, texture.Width);
        Assert.Equal(1, texture.LevelCount);

        using var png = new MemoryStream();
        texture.SaveAsPng(png, 4, 4);
        png.Position = 0;
        using Texture2D decoded = Texture2D.FromStream(device, png);
        Assert.Equal(4, decoded.Height);

        using var target = new RenderTarget2D(device, 8, 8, false, SurfaceFormat.Color, DepthFormat.Depth24);
        Assert.Equal(8, target.Width);

        using var cube = new TextureCube(device, 4, false, SurfaceFormat.Color);
        cube.SetData(CubeMapFace.PositiveX, pixels);
        var face = new Color[16];
        cube.GetData(CubeMapFace.PositiveX, face);
        Assert.Equal(pixels, face);

        using var volume = new Texture3D(device, 2, 2, 4, false, SurfaceFormat.Color);
        volume.SetData(pixels);
        var box = new Color[16];
        volume.GetData(box);
        Assert.Equal(pixels, box);

        var triangle = new[]
        {
            new VertexPositionColor(Vector3.Zero, Color.Red),
            new VertexPositionColor(Vector3.UnitX, Color.Green),
            new VertexPositionColor(Vector3.UnitY, Color.Blue),
        };
        using var vertices = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, 3, BufferUsage.None);
        vertices.SetData(triangle);
        var vertexBack = new VertexPositionColor[3];
        vertices.GetData(vertexBack);
        Assert.Equal(triangle, vertexBack);

        using var dynamicVertices = new DynamicVertexBuffer(device, VertexPositionColor.VertexDeclaration, 3, BufferUsage.WriteOnly);
        dynamicVertices.SetData(triangle, 0, 3, SetDataOptions.Discard);

        short[] order = [0, 1, 2];
        using var indices = new IndexBuffer(device, IndexElementSize.SixteenBits, 3, BufferUsage.None);
        indices.SetData(order);
        var indexBack = new short[3];
        indices.GetData(indexBack);
        Assert.Equal(order, indexBack);

        using var dynamicIndices = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, 3, BufferUsage.WriteOnly);
        dynamicIndices.SetData(order, 0, 3, SetDataOptions.Discard);

        using var basic = new BasicEffect(device) { TextureEnabled = true, Texture = texture };
        using Effect clone = basic.Clone();
        Assert.IsType<BasicEffect>(clone);

        // What a loading thread configures next, through calls the facade does not move itself:
        // CNA runs them on the game thread (C ABI 0.39.0, CSX-101) -- and the device queries, one
        // hop each because the device CNA lends lasts one callback scope.
        Viewport viewport = device.Viewport;
        Assert.True(viewport.Width > 0);
        Assert.Equal(viewport.Width, device.PresentationParameters.BackBufferWidth);
        Matrix world = Matrix.CreateTranslation(1f, 2f, 3f);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, viewport.AspectRatio, 0.1f, 100f);
        basic.World = world;
        basic.View = Matrix.CreateLookAt(new Vector3(0f, 0f, 5f), Vector3.Zero, Vector3.Up);
        basic.Projection = projection;
        Assert.Equal(world, basic.World);
        Assert.Equal(projection, basic.Projection);
        basic.EnableDefaultLighting();
        basic.DirectionalLight0.Direction = Vector3.Down;
        Assert.True(basic.LightingEnabled);
        Assert.Equal(Vector3.Down, basic.DirectionalLight0.Direction);
        using var alphaTest = new AlphaTestEffect(device);
        using var dualTexture = new DualTextureEffect(device);
        using var environmentMap = new EnvironmentMapEffect(device);
        using var skinned = new SkinnedEffect(device);
        using var batch = new SpriteBatch(device);
        using var query = new OcclusionQuery(device);

        SpriteFont font = content.Load<SpriteFont>("FontCalibri14");
        Assert.True(font.MeasureString("CNA").X > 0);
    }

    [global::CNA.Integration.Tests.NativeFact]
    public void AWorkerThread_CreatesResources_MovesTheirData_AndLoadsContent_WhileTheGameRuns()
    {
        using var game = new LoadingGame();
        game.Run();

        Assert.True(game.WorkerFinished, "the worker did not finish within 1200 frames");
        Assert.NotEqual(game.GameThreadId, game.WorkerThreadId);
        if (game.WorkerFailure is not null)
        {
            throw new Xunit.Sdk.XunitException("the worker failed: " + game.WorkerFailure);
        }
    }

    /// <summary>
    /// escape-from-enceladus's title screen starts three threads at once, one per save slot; each
    /// selects the shared static StorageDevice under a lock if no thread has yet, then opens a
    /// container on it. Before CNA.NET selected the device on the game thread (CSX-109), the threads
    /// that did not select it were refused (CNA_RESULT_THREAD).
    /// </summary>
    private sealed class SavingGame : XnaGame
    {
        private const string ContainerName = "cna-cs CompatWorkerThreadTests";
        private static readonly object Lock = new();
        private StorageDevice? _device;
        private Thread[]? _workers;
        private int _frames;

        public SavingGame()
        {
            _ = new GraphicsDeviceManager(this);
        }

        public Exception? WorkerFailure { get; private set; }

        public int Opened;

        protected override void Update(GameTime gameTime)
        {
            if (_workers is null)
            {
                _workers = Enumerable.Range(0, 3).Select(_ => new Thread(Load) { IsBackground = true }).ToArray();
                Array.ForEach(_workers, worker => worker.Start());
            }

            if (_workers.All(worker => !worker.IsAlive) || ++_frames > 1200)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        private void Load()
        {
            try
            {
                lock (Lock)
                {
                    if (_device is null)
                    {
                        IAsyncResult selected = StorageDevice.BeginShowSelector(null, null);
                        selected.AsyncWaitHandle.WaitOne();
                        _device = StorageDevice.EndShowSelector(selected);
                    }
                }

                IAsyncResult opened = _device.BeginOpenContainer(ContainerName, null, null);
                opened.AsyncWaitHandle.WaitOne();
                using StorageContainer container = _device.EndOpenContainer(opened);
                string file = $"slot{Environment.CurrentManagedThreadId}.sav";
                byte[] save = BitConverter.GetBytes(Environment.CurrentManagedThreadId);
                using (Stream stream = container.CreateFile(file))
                {
                    stream.Write(save);
                }

                using (Stream stream = container.OpenFile(file, FileMode.Open))
                {
                    var read = new byte[save.Length];
                    Assert.Equal(save.Length, stream.Read(read));
                    Assert.Equal(save, read);
                }

                container.DeleteFile(file);
                Interlocked.Increment(ref Opened);
            }
            catch (Exception ex)
            {
                WorkerFailure = ex;
            }
        }
    }

    [global::CNA.Integration.Tests.NativeFact]
    public void WorkerThreads_ShareOneStorageDevice_WhileTheGameRuns()
    {
        using var game = new SavingGame();
        game.Run();

        if (game.WorkerFailure is not null)
        {
            throw new Xunit.Sdk.XunitException("a worker failed: " + game.WorkerFailure);
        }

        Assert.Equal(3, game.Opened);
    }
}
