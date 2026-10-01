using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors;

/// <summary>One accelerometer reading, in g.</summary>
public struct AccelerometerReading : ISensorReading
{
    public Vector3 Acceleration { get; internal set; }

    public DateTimeOffset Timestamp { get; internal set; }
}
