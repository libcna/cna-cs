// A library as XNA middleware shipped: an x86 Windows Game Library compiled against XNA Game Studio
// 4.0's own reference assemblies (build.sh), so every XNA type it names is
// "Microsoft.Xna.Framework*, Version=4.0.0.0, PublicKeyToken=842cf8be1de50553". XnaBinaryLibraryTests
// run the checked-in binary on CNA.NET.
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XnaBinaryLibrary
{
    public static class Physics
    {
        public static Vector3 Gravity = new Vector3(0f, -9.8f, 0f);

        public static Vector3 Step(Vector3 position, ref Vector3 velocity, GameTime time)
        {
            float seconds = (float)time.ElapsedGameTime.TotalSeconds;
            velocity += Gravity * seconds;
            return position + velocity * seconds;
        }

        public static Matrix World(Vector3 position, Quaternion orientation)
        {
            return Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position);
        }

        public static bool Touches(BoundingSphere sphere, BoundingBox box)
        {
            return sphere.Intersects(box);
        }

        public static Color Fade(Color color, float amount)
        {
            return Color.Lerp(color, Color.Transparent, MathHelper.Clamp(amount, 0f, 1f));
        }
    }

    public struct Particle : IVertexType
    {
        public Vector3 Position;
        public Color Color;

        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0));

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return Declaration; }
        }
    }

    public sealed class Body
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public BoundingSphere Bounds;
    }

    public sealed class BodyReader : ContentTypeReader<Body>
    {
        protected override Body Read(ContentReader input, Body existingInstance)
        {
            Body body = existingInstance ?? new Body();
            body.Position = input.ReadVector3();
            body.Velocity = input.ReadVector3();
            body.Bounds = new BoundingSphere(body.Position, input.ReadSingle());
            return body;
        }
    }
}
