namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// XNA's packet writer: a <see cref="BinaryWriter"/> over its own memory stream, adding the XNA
/// value types in their field order. Sending one sends its bytes and then empties it.
/// </summary>
public class PacketWriter : BinaryWriter
{
    public PacketWriter()
        : this(0)
    {
    }

    public PacketWriter(int capacity)
        : base(new MemoryStream(capacity))
    {
    }

    public int Length => (int)BaseStream.Length;

    public int Position
    {
        get => (int)BaseStream.Position;
        set => BaseStream.Position = value;
    }

    /// <summary>The written bytes; valid up to <see cref="Length"/>.</summary>
    internal byte[] ByteArray => ((MemoryStream)BaseStream).GetBuffer();

    internal void Clear()
    {
        var stream = (MemoryStream)BaseStream;
        stream.SetLength(0);
        stream.Position = 0;
    }

    public void Write(Vector2 value)
    {
        Write(value.X);
        Write(value.Y);
    }

    public void Write(Vector3 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
    }

    public void Write(Vector4 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    public void Write(Matrix value)
    {
        Write(value.M11);
        Write(value.M12);
        Write(value.M13);
        Write(value.M14);
        Write(value.M21);
        Write(value.M22);
        Write(value.M23);
        Write(value.M24);
        Write(value.M31);
        Write(value.M32);
        Write(value.M33);
        Write(value.M34);
        Write(value.M41);
        Write(value.M42);
        Write(value.M43);
        Write(value.M44);
    }

    public void Write(Quaternion value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    public void Write(Color value) => Write(value.PackedValue);

    /// <summary>The raw IEEE bits, as XNA writes them, so a NaN payload survives the trip.</summary>
    public override void Write(float value) => Write(BitConverter.SingleToUInt32Bits(value));

    public override void Write(double value) => Write(BitConverter.DoubleToUInt64Bits(value));
}
