using System.Collections.ObjectModel;
using CNA.Integration.Tests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Xunit;
using Xunit.Abstractions;

namespace CnaDotnet.GamerServices.IntegrationTests;

[Collection(GamerServicesCollection.Name)]
public class AvatarTests(GamerServicesGameFixture fixture, ITestOutputHelper output)
{
    [NativeFact]
    public void Description_FromBytes_RoundTripsAndValidatesLikeXna()
    {
        AvatarDescription random = AvatarDescription.CreateRandom(AvatarBodyType.Male);
        Assert.True(random.IsValid);
        Assert.Equal(AvatarBodyType.Male, random.BodyType);
        output.WriteLine($"height={random.Height}");

        byte[] bytes = random.Description;
        Assert.Equal(1021, bytes.Length);
        Assert.NotSame(bytes, random.Description);
        bytes[1] ^= 0xFF;
        Assert.NotEqual(bytes, random.Description);
        bytes[1] ^= 0xFF;

        var copy = new AvatarDescription(bytes);
        Assert.Equal(bytes, copy.Description);
        Assert.Equal(random.BodyType, copy.BodyType);
        Assert.Equal(random.Height, copy.Height);

        // Valid is decided by the first byte, never by whether the bytes can be read.
        bytes[0] = 0;
        Assert.False(new AvatarDescription(bytes).IsValid);

        Assert.Equal("data", Assert.Throws<ArgumentNullException>(() => new AvatarDescription(null!)).ParamName);
        Assert.Equal("data", Assert.Throws<ArgumentException>(() => new AvatarDescription(new byte[10])).ParamName);
        Assert.Equal(
            "bodyType",
            Assert.Throws<ArgumentOutOfRangeException>(() => AvatarDescription.CreateRandom((AvatarBodyType)2)).ParamName);
        Assert.True(AvatarDescription.CreateRandom().IsValid);
    }

    [NativeFact]
    public void Description_FromASignedInGamer_IsThatPlayersOneObject()
    {
        fixture.InsideAFrame(_ =>
        {
            SignedInGamer gamer = Gamer.SignedInGamers[PlayerIndex.One];
            int callbacks = 0;
            IAsyncResult first = AvatarDescription.BeginGetFromGamer(gamer, result => callbacks++, "state");
            Assert.Equal(1, callbacks);
            Assert.True(first.IsCompleted);
            Assert.Equal("state", first.AsyncState);

            AvatarDescription description = AvatarDescription.EndGetFromGamer(first);
            Assert.Same(description, AvatarDescription.EndGetFromGamer(first));
            Assert.Same(description, AvatarDescription.EndGetFromGamer(AvatarDescription.BeginGetFromGamer(gamer, null!, null!)));
            output.WriteLine($"valid={description.IsValid} body={description.BodyType}");

            Assert.Throws<ArgumentException>(() => AvatarDescription.EndGetFromGamer(gamer.BeginGetProfile(null!, null!)));
            Assert.Equal("gamer", Assert.Throws<ArgumentNullException>(() => AvatarDescription.BeginGetFromGamer(null!, null!, null!)).ParamName);
        });
    }

    [NativeFact]
    public void Animation_KeepsItsStateAndOneBoneCollection()
    {
        var animation = new AvatarAnimation(AvatarAnimationPreset.Celebrate);
        ReadOnlyCollection<Matrix> bones = animation.BoneTransforms;
        Assert.Equal(AvatarRenderer.BoneCount, bones.Count);
        Assert.Equal(TimeSpan.Zero, animation.CurrentPosition);
        output.WriteLine($"length={animation.Length} mouth={animation.Expression.Mouth}");

        animation.Update(TimeSpan.FromSeconds(0.5), loop: true);
        Assert.Same(bones, animation.BoneTransforms);
        Assert.True(animation.CurrentPosition >= TimeSpan.Zero);
        Assert.True(animation.Length == TimeSpan.Zero || animation.CurrentPosition <= animation.Length);

        animation.CurrentPosition = TimeSpan.Zero;
        Assert.Equal(TimeSpan.Zero, animation.CurrentPosition);

        animation.Dispose();
        animation.Dispose();
        Assert.True(animation.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => animation.Update(TimeSpan.Zero, false));
        // XNA's getters are fields: they keep answering after Dispose.
        Assert.Equal(AvatarRenderer.BoneCount, animation.BoneTransforms.Count);
        _ = animation.Length;
        _ = animation.Expression;
    }

    [NativeFact]
    public void Renderer_FinishesLoadingAndThenHasABindPose()
    {
        AvatarRenderer? renderer = null;
        AvatarRendererState state = AvatarRendererState.Loading;
        fixture.InsideAFrame(_ => renderer = new AvatarRenderer(AvatarDescription.CreateRandom(), useLoadingEffect: true));
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (state == AvatarRendererState.Loading && clock.Elapsed < TimeSpan.FromSeconds(20))
        {
            using var animation = new AvatarAnimation(AvatarAnimationPreset.Stand1);
            fixture.InsideAFrame(_ =>
            {
                renderer!.Draw(animation);
                state = renderer.State;
            });
        }

        output.WriteLine($"state={state} after {clock.ElapsedMilliseconds} ms");
        Assert.NotEqual(AvatarRendererState.Loading, state);
        fixture.InsideAFrame(_ =>
        {
            if (state == AvatarRendererState.Ready)
            {
                ReadOnlyCollection<Matrix> bindPose = renderer!.BindPose;
                Assert.Equal(AvatarRenderer.BoneCount, bindPose.Count);
                Assert.Same(bindPose, renderer.BindPose);
                Assert.Contains(bindPose, bone => bone != Matrix.Identity);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => renderer!.BindPose);
            }

            renderer!.Dispose();
        });
    }

    [NativeFact]
    public void Renderer_DrawsAnAnimationInsideAFrame()
    {
        fixture.InsideAFrame(_ =>
        {
            using var animation = new AvatarAnimation(AvatarAnimationPreset.Stand0);
            var renderer = new AvatarRenderer(AvatarDescription.CreateRandom(AvatarBodyType.Female), useLoadingEffect: false);

            Assert.Equal(AvatarRenderer.BoneCount, renderer.ParentBones.Count);
            Assert.Contains(-1, renderer.ParentBones);
            Assert.All(renderer.ParentBones, parent => Assert.InRange(parent, -1, AvatarRenderer.BoneCount - 1));
            Assert.Equal(Matrix.Identity, renderer.World);

            renderer.World = Matrix.CreateTranslation(1, 2, 3);
            renderer.View = Matrix.CreateLookAt(new Vector3(0, 1, 3), new Vector3(0, 1, 0), Vector3.Up);
            renderer.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4f / 3f, 0.1f, 100f);
            renderer.LightDirection = Vector3.Down;
            Assert.Equal(Matrix.CreateTranslation(1, 2, 3), renderer.World);

            AvatarRendererState state = renderer.State;
            output.WriteLine($"state={state}");
            if (state == AvatarRendererState.Ready)
            {
                Assert.Equal(AvatarRenderer.BoneCount, renderer.BindPose.Count);
                Assert.Same(renderer.BindPose, renderer.BindPose);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => renderer.BindPose);
            }

            renderer.Draw(animation);
            renderer.Draw(animation.BoneTransforms, new AvatarExpression { Mouth = AvatarMouth.Happy });
            Assert.Equal("animation", Assert.Throws<ArgumentNullException>(() => renderer.Draw((IAvatarAnimation)null!)).ParamName);
            Assert.Equal("bones", Assert.Throws<ArgumentNullException>(() => renderer.Draw(null!, default)).ParamName);
            Assert.Equal("bones", Assert.Throws<ArgumentException>(() => renderer.Draw(new Matrix[3], default)).ParamName);

            renderer.Dispose();
            Assert.True(renderer.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => renderer.State);
            Assert.Throws<ObjectDisposedException>(() => renderer.Draw(animation));
            Assert.Equal(Matrix.CreateTranslation(1, 2, 3), renderer.World);
        });
    }
}
