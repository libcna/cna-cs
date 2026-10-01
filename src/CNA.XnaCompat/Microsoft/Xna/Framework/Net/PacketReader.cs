namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// XNA's packet reader: a <see cref="BinaryReader"/> over its own memory stream, which a receive
/// resizes to exactly the packet it delivers.
/// </summary>
public class PacketReader : BinaryReader
{
    public PacketReader()
        : this(0)
    {
    }

    public PacketReader(int capacity)
        : base(new MemoryStream(capacity))
    {
    }

    public int Length => (int)BaseStream.Length;

    public int Position
    {
        get => (int)BaseStream.Position;
        set => BaseStream.Position = value;
    }

    /// <summary>Replaces the contents with a received packet and rewinds.</summary>
    internal void SetPacket(byte[] packet, int count)
    {
        var stream = (MemoryStream)BaseStream;
        stream.SetLength(count);
        stream.Position = 0;
        stream.Write(packet, 0, count);
        stream.Position = 0;
    }

    public Vector2 ReadVector2()
    {
        Vector2 value = default;
        value.X = ReadSingle();
        value.Y = ReadSingle();
        return value;
    }

    public Vector3 ReadVector3()
    {
        Vector3 value = default;
        value.X = ReadSingle();
        value.Y = ReadSingle();
        value.Z = ReadSingle();
        return value;
    }

    public Vector4 ReadVector4()
    {
        Vector4 value = default;
        value.X = ReadSingle();
        value.Y = ReadSingle();
        value.Z = ReadSingle();
        value.W = ReadSingle();
        return value;
    }

    public Matrix ReadMatrix()
    {
        Matrix value = default;
        value.M11 = ReadSingle();
        value.M12 = ReadSingle();
        value.M13 = ReadSingle();
        value.M14 = ReadSingle();
        value.M21 = ReadSingle();
        value.M22 = ReadSingle();
        value.M23 = ReadSingle();
        value.M24 = ReadSingle();
        value.M31 = ReadSingle();
        value.M32 = ReadSingle();
        value.M33 = ReadSingle();
        value.M34 = ReadSingle();
        value.M41 = ReadSingle();
        value.M42 = ReadSingle();
        value.M43 = ReadSingle();
        value.M44 = ReadSingle();
        return value;
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion value = default;
        value.X = ReadSingle();
        value.Y = ReadSingle();
        value.Z = ReadSingle();
        value.W = ReadSingle();
        return value;
    }

    public Color ReadColor()
    {
        Color value = default;
        value.PackedValue = ReadUInt32();
        return value;
    }

    public override float ReadSingle() => BitConverter.UInt32BitsToSingle(ReadUInt32());

    public override double ReadDouble() => BitConverter.UInt64BitsToDouble(ReadUInt64());
}
