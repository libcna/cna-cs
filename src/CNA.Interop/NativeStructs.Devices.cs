using System.Runtime.InteropServices;

namespace CNA.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct CnaDateTimeOffset
{
    public long Ticks;
    public long OffsetTicks;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAccelerometerReading
{
    public uint StructSize;
    public uint StructVersion;
    public CnaDateTimeOffset Timestamp;
    public CnaVector3 Acceleration;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAccelerometerReadingEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaDateTimeOffset Timestamp;
    public double X;
    public double Y;
    public double Z;
}
