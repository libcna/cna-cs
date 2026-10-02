using CNA.Graphics;
using Xunit;
using Xunit.Abstractions;

namespace CNA.Integration.Tests;

/// <summary>
/// CSX-130: an Immediate batch draws each sprite when it is given, as XNA's did, so what a game sets
/// on the device or on its effect between two draws applies to the second only. Asteria's blend demo
/// sets a texture and a blend amount on its effect before each of three clouds; with the batch held
/// to <c>End</c>, all three drew with the last values.
/// </summary>
[Collection(NativeGameCollection.Name)]
public class ImmediateSpriteBatchTests(ITestOutputHelper output, NativeGameFixture fixture)
{
    [NativeFact]
    public void ImmediateBatch_DrawsEachSpriteWithTheStateSetBeforeIt()
    {
        fixture.InsideAFrame(game =>
        {
            GraphicsDevice device = game.GraphicsDevice;
            if (!CnaNativeProbe.RequireRenderTargetReadback(device, output))
            {
                return;
            }

            using var green = new Texture2D(device, 1, 1);
            green.SetData([new Color(0, 255, 0, 128)]);
            using var red = new Texture2D(device, 1, 1);
            red.SetData([new Color(255, 0, 0, 255)]);
            using var target = new RenderTarget2D(device, 4, 4);

            device.SetRenderTarget(target);
            try
            {
                device.Clear(new Color(0, 0, 0, 255));
                using var batch = new SpriteBatch(device);
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                batch.Draw(green, new Vector2(0f, 0f), new Color(255, 255, 255, 255));
                device.BlendState = BlendState.Additive;
                batch.Draw(red, new Vector2(0f, 0f), new Color(255, 255, 255, 255));
                batch.End();
            }
            finally
            {
                device.SetRenderTarget(null);
                device.BlendState = BlendState.Opaque;
            }

            var pixels = new Color[16];
            target.GetData(pixels);

            // Opaque wrote the green texel whole; Additive then added red onto it. Drawn together at
            // End, both would have been additive: half the green, then the red.
            Assert.Equal(255, pixels[0].R);
            Assert.Equal(255, pixels[0].G);
            Assert.Equal(0, pixels[0].B);
        });
    }
}
