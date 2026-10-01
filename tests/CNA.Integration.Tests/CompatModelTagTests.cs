using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using XnaGame = Microsoft.Xna.Framework.Game;

// NOT under CNA, for the reason CompatLayerIntegrationTests records.
namespace CnaCs.Integration.Tests.Compat;

/// <summary>A game-defined tag, read by the game's own reader -- what HeightmapCollision's
/// <c>HeightMapInfo</c> is to its terrain.</summary>
public sealed class ElevationTag
{
    public int Height { get; init; }
}

public sealed class ElevationTagReader : ContentTypeReader<ElevationTag>
{
    protected override ElevationTag Read(ContentReader input, ElevationTag existingInstance) =>
        new() { Height = input.ReadInt32() };
}

/// <summary>
/// A <c>Model</c> whose tag is of the game's own type loads through the game's own reader
/// (cna-cs CSX-089), as HeightmapCollision's terrain does.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class CompatModelTagTests
{
    private sealed class ModelProbe : XnaGame
    {
        private readonly string _contentRoot;

        public ModelProbe(string contentRoot)
        {
            _contentRoot = contentRoot;
            _ = new GraphicsDeviceManager(this);
        }

        public Model? Loaded { get; private set; }

        public Exception? Failure { get; private set; }

        protected override void LoadContent()
        {
            Content.RootDirectory = _contentRoot;
            try
            {
                Loaded = Content.Load<Model>("tagged");
            }
            catch (Exception ex)
            {
                Failure = ex;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Exit();
            base.Update(gameTime);
        }
    }

    [global::CNA.Integration.Tests.NativeFact]
    public void Model_WhoseTagIsOfTheGamesOwnType_LoadsThroughTheGamesReader()
    {
        string root = Path.Combine(Path.GetTempPath(), "cna-cs-model-tag-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllBytes(Path.Combine(root, "tagged.xnb"), ModelWithElevationTag(1234));
            using var game = new ModelProbe(root);
            game.RunOneFrame();

            Assert.Null(game.Failure);
            Model model = Assert.IsType<Model>(game.Loaded);
            Assert.Equal(1234, Assert.IsType<ElevationTag>(model.Tag).Height);
            Assert.Equal("Root", Assert.Single(model.Bones).Name);
            Assert.Empty(model.Meshes);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>A one-bone, meshless model whose own tag uses the game's reader.</summary>
    private static byte[] ModelWithElevationTag(int height)
    {
        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            string[] readers =
            [
                "Microsoft.Xna.Framework.Content.ModelReader",
                "Microsoft.Xna.Framework.Content.StringReader",
                typeof(ElevationTagReader).FullName + ", " + typeof(ElevationTagReader).Assembly.GetName().Name,
            ];
            writer.Write7BitEncodedInt(readers.Length);
            foreach (string name in readers)
            {
                writer.Write(name);
                writer.Write(0);
            }

            writer.Write7BitEncodedInt(0);                          // no shared resources
            writer.Write7BitEncodedInt(1);                          // the root: a Model
            writer.Write(1u);                                       // one bone
            writer.Write7BitEncodedInt(2); writer.Write("Root");    // its name
            for (int i = 0; i < 16; i++)
            {
                writer.Write(i % 5 == 0 ? 1f : 0f);                 // identity
            }

            writer.Write((byte)0);                                  // no parent
            writer.Write(0u);                                       // no children
            writer.Write(0u);                                       // no meshes
            writer.Write((byte)1);                                  // root bone reference
            writer.Write7BitEncodedInt(3); writer.Write(height);    // the tag, by the game's reader
        }

        byte[] body = payload.ToArray();
        using var container = new MemoryStream();
        using (var writer = new BinaryWriter(container, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("XNBw"u8);
            writer.Write((byte)5);
            writer.Write((byte)0);
            writer.Write(10 + body.Length);
            writer.Write(body);
        }

        return container.ToArray();
    }
}
