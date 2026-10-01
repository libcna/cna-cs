namespace Microsoft.Devices.Sensors;

public enum SensorState
{
    NotSupported = 0,
    Ready = 1,
    Initializing = 2,
    NoData = 3,
    NoPermissions = 4,
    Disabled = 5,
}
