using CNA.Graphics;
using Xunit;
using Xunit.Abstractions;

namespace CNA.Integration.Tests;

/// <summary>
/// CSX-135: XNA's <c>Apply</c> binds a stock effect's texture to the device's sampler, and a draw
/// samples whatever that slot holds -- so a texture a game sets on <c>GraphicsDevice.Textures</c>
/// after <c>Apply</c> is the one drawn, as XNA's own SpriteBatch relies on. jaquadro/LilyPath's
/// DrawBatch applies a texture-enabled BasicEffect with no texture, then sets its brush texture on
/// the device; CNA drew with the effect's own (missing) texture, and its whole logo came out black.
/// </summary>
[Collection(NativeGameCollection.Name)]
public class EffectDeviceTextureTests(ITestOutputHelper output, NativeGameFixture fixture)
{
    private static readonly Color Background = new(7, 199, 53, 255);
    private static readonly Color Vertex = new(0, 128, 255, 255);
    private static readonly Color OwnTexel = new(255, 64, 128, 255);

    [NativeFact]
    public void BasicEffect_SamplesTheTextureSetOnTheDeviceAfterApply()
    {
        DrawWithBasicEffect(withOwnTexture: false, deviceTextureAfterApply: true, expected: Vertex);
    }

    [NativeFact]
    public void BasicEffect_TheDeviceTextureSetAfterApplyReplacesTheEffectsOwn()
    {
        DrawWithBasicEffect(withOwnTexture: true, deviceTextureAfterApply: true, expected: Vertex);
    }

    [NativeFact]
    public void BasicEffect_ApplyBindsItsOwnTextureOverTheDevicesEarlierOne()
    {
        // The vertex colour modulated by the effect's own texel: (0, 128 * 64 / 255, 255 * 128 / 255).
        DrawWithBasicEffect(withOwnTexture: true, deviceTextureAfterApply: false, expected: new Color(0, 32, 128, 255));
    }

    /// <summary>
    /// A quad in <see cref="Vertex"/> colour through a texture-enabled BasicEffect whose own texture
    /// is an <see cref="OwnTexel"/> texel, or none; a white texel goes on the device either after
    /// Apply or, for Apply to replace, before it.
    /// </summary>
    private void DrawWithBasicEffect(bool withOwnTexture, bool deviceTextureAfterApply, Color expected)
    {
        fixture.InsideAFrame(game =>
        {
            GraphicsDevice device = game.GraphicsDevice;
            if (!CnaNativeProbe.RequireRenderTargetReadback(device, output))
            {
                return;
            }

            using var white = new Texture2D(device, 1, 1);
            white.SetData([new Color(255, 255, 255, 255)]);
            using var own = new Texture2D(device, 1, 1);
            own.SetData([OwnTexel]);
            using var target = new RenderTarget2D(device, 4, 4);
            using var effect = new BasicEffect(device)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0f, 4f, 4f, 0f, -1f, 1f),
                Texture = withOwnTexture ? own : null,
            };
            VertexPositionColorTexture[] quad =
            [
                new(new Vector3(0f, 0f, 0f), Vertex, Vector2.Zero),
                new(new Vector3(4f, 0f, 0f), Vertex, Vector2.UnitX),
                new(new Vector3(0f, 4f, 0f), Vertex, Vector2.UnitY),
                new(new Vector3(4f, 4f, 0f), Vertex, Vector2.One),
            ];
            short[] indices = [0, 1, 2, 1, 3, 2];

            device.SetRenderTarget(target);
            try
            {
                device.Clear(Background);
                device.BlendState = BlendState.Opaque;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.PointClamp;
                if (!deviceTextureAfterApply)
                {
                    device.Textures[0] = white;
                }

                effect.CurrentTechnique.Passes[0].Apply();
                if (deviceTextureAfterApply)
                {
                    device.Textures[0] = white;
                }

                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, quad, 0, 4, indices, 0, 2);
            }
            finally
            {
                device.SetRenderTarget(null);
                device.Textures[0] = null;
            }

            var pixels = new Color[16];
            target.GetData(pixels);
            Color drawn = pixels[5];
            output.WriteLine($"drawn {drawn}, expected {expected}");
            Assert.InRange(drawn.R, expected.R - 1, expected.R + 1);
            Assert.InRange(drawn.G, expected.G - 1, expected.G + 1);
            Assert.InRange(drawn.B, expected.B - 1, expected.B + 1);
        });
    }
}
