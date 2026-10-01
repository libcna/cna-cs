using System.Runtime.InteropServices;

namespace CNA.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct CnaQualityOfService
{
    public uint StructSize;
    public uint StructVersion;
    public byte IsAvailable;
    public CnaReservedBytes7 Reserved;
    public long AverageRoundtripTicks;
    public long MinimumRoundtripTicks;
    public int BytesPerSecondDownstream;
    public int BytesPerSecondUpstream;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaOptionalInt32
{
    public byte HasValue;
    public CnaReservedBytes3 Reserved;
    public int Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGameEndedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGameStartedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGamerJoinedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaHandle Gamer;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaGamerLeftEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaHandle Gamer;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaHostChangedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaHandle OldHost;
    public CnaHandle NewHost;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaNetworkSessionEndedEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public uint EndReason;
    public CnaReservedBytes4 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaWriteLeaderboardsEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public CnaHandle Gamer;
    public byte IsLeaving;
    public CnaReservedBytes7 Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaAvailableNetworkSessionCreateInfo
{
    public uint StructSize;
    public uint StructVersion;
    public int CurrentGamerCount;
    public int OpenPrivateGamerSlots;
    public int OpenPublicGamerSlots;
    public uint SessionType;
    public ushort HostPort;
    public CnaReservedBytes6 Reserved;
    public CnaStringView HostGamertag;
    public CnaStringView HostAddress;
    public CnaHandle SessionProperties;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CnaNetworkEventInfo
{
    public uint StructSize;
    public uint StructVersion;
    public uint Type;
    public uint Reliable;
    public uint State;
    public uint Reason;
    public CnaHandle Gamer;
    public CnaHandle Sender;
    public nint Packet;
    public ulong PacketByteCount;
}
