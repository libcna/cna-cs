using CNA.Interop;

namespace Microsoft.Devices;

/// <summary>The phone's environment queries.</summary>
public static class Environment
{
    /// <summary><see cref="Microsoft.Devices.DeviceType.Device"/> on a mobile platform, and
    /// <see cref="Microsoft.Devices.DeviceType.Emulator"/> elsewhere: a desktop or browser host plays
    /// the part the Windows Phone emulator did on a development PC, which is the branch phone games
    /// take for it (CNA's own answer, the same for the C++ ports).</summary>
    public static DeviceType DeviceType
    {
        get
        {
            DevicesInterop.Check(Native.cna_environment_get_device_type(out uint type), nameof(DeviceType));
            return (DeviceType)type;
        }
    }
}
