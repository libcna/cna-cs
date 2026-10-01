using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaBinaryLibrary;
using Xunit;

namespace CnaCs.XnaCompat.Tests;

/// <summary>
/// A library compiled against XNA 4.0 itself (tests/fixtures/xna-binary-library, a checked-in
/// binary) runs on CNA.NET through the XNA-named forwarding assemblies in src/XnaAssemblies. That
/// this file compiles at all is the compiler half: it passes CNA.XnaCompat's types where the
/// library's signatures name XNA's.
/// </summary>
public class XnaBinaryLibraryTests
{
    [Fact]
    public void Library_CompiledAgainstXna_LoadsThroughTheXnaNames_AndItsTypesAreTheFacadeTypes()
    {
        // As compiled: XNA's own identity, Microsoft's public key token included, for x86 only.
        string compiled = Path.Combine(FixtureDirectory(), "XnaBinaryLibrary.dll");
        AssemblyName compiledFramework = Assert.Single(ReferencesOf(compiled), r => r.Name == "Microsoft.Xna.Framework");
        Assert.Equal("842cf8be1de50553", Convert.ToHexString(compiledFramework.GetPublicKeyToken()!).ToLowerInvariant());

        // As built against: CNA.XnaCompat.targets' copy names XNA's assembly without the token, so
        // the unsigned XNA-named forwarders answer it for the compiler as they do at run time.
        AssemblyName framework = Assert.Single(
            typeof(Physics).Assembly.GetReferencedAssemblies(), r => r.Name == "Microsoft.Xna.Framework");
        Assert.Equal(new Version(4, 0, 0, 0), framework.Version);
        Assert.Empty(framework.GetPublicKeyToken() ?? []);

        Assert.Same(typeof(Vector3), typeof(Physics).GetField(nameof(Physics.Gravity))!.FieldType);
    }

    private static AssemblyName[] ReferencesOf(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new System.Reflection.PortableExecutable.PEReader(stream);
        var metadata = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
        return metadata.AssemblyReferences.Select(h => metadata.GetAssemblyReference(h).GetAssemblyName()).ToArray();
    }

    private static string FixtureDirectory()
    {
        string? directory = AppContext.BaseDirectory;
        while (directory is not null && !Directory.Exists(Path.Combine(directory, "tests", "fixtures", "xna-binary-library")))
            directory = Path.GetDirectoryName(directory);
        return Path.Combine(directory ?? throw new DirectoryNotFoundException("tests/fixtures"), "tests", "fixtures", "xna-binary-library");
    }

    [Fact]
    public void Library_ComputesWithFacadeMathTypes()
    {
        var velocity = new Vector3(1f, 0f, 0f);
        var time = new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.5));

        Vector3 position = Physics.Step(new Vector3(0f, 10f, 0f), ref velocity, time);

        Assert.Equal(new Vector3(1f, -4.9f, 0f), velocity);
        Assert.Equal(new Vector3(0.5f, 7.55f, 0f), position);

        Quaternion turn = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2);
        Assert.Equal(Matrix.CreateFromQuaternion(turn) * Matrix.CreateTranslation(1f, 2f, 3f),
                     Physics.World(new Vector3(1f, 2f, 3f), turn));

        Assert.True(Physics.Touches(new BoundingSphere(new Vector3(2f, 0f, 0f), 1.5f), new BoundingBox(-Vector3.One, Vector3.One)));
        Assert.False(Physics.Touches(new BoundingSphere(new Vector3(3f, 0f, 0f), 1.5f), new BoundingBox(-Vector3.One, Vector3.One)));
        Assert.Equal(Color.Lerp(Color.Red, Color.Transparent, 1f), Physics.Fade(Color.Red, 2f));
    }

    [Fact]
    public void LibraryVertexType_ImplementsTheFacadeInterface()
    {
        IVertexType particle = new Particle();

        VertexElement[] elements = particle.VertexDeclaration.GetVertexElements();

        Assert.Equal(16, particle.VertexDeclaration.VertexStride);
        Assert.Equal(VertexElementUsage.Color, elements[1].VertexElementUsage);
    }

    [Fact]
    public void LibraryContentTypeReader_ReadsAnXnb()
    {
        using var content = new MemoryContentManager(BuildBodyAsset());

        Body body = content.Load<Body>("body");

        Assert.Equal(new Vector3(1f, 2f, 3f), body.Position);
        Assert.Equal(new Vector3(0f, -1f, 0f), body.Velocity);
        Assert.Equal(new BoundingSphere(new Vector3(1f, 2f, 3f), 0.5f), body.Bounds);
    }

    private static byte[] BuildBodyAsset()
    {
        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write7BitEncodedInt(1);
            // As XNA's pipeline names a runtime reader: by its assembly's full name.
            writer.Write("XnaBinaryLibrary.BodyReader, XnaBinaryLibrary, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            writer.Write(0);
            writer.Write7BitEncodedInt(0); // no shared resources
            writer.Write7BitEncodedInt(1); // BodyReader
            foreach (float value in new[] { 1f, 2f, 3f, 0f, -1f, 0f, 0.5f })
                writer.Write(value);
        }

        byte[] bytes = payload.ToArray();
        using var container = new MemoryStream();
        using (var writer = new BinaryWriter(container, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("XNBw"u8);
            writer.Write((byte)5);
            writer.Write((byte)0);
            writer.Write(10 + bytes.Length);
            writer.Write(bytes);
        }

        return container.ToArray();
    }

    private sealed class MemoryContentManager(byte[] asset) : ContentManager(new NullServiceProvider())
    {
        protected override Stream OpenStream(string assetName) => new MemoryStream(asset, writable: false);
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
