namespace Microsoft.Devices.Sensors;

/// <summary>The Windows Phone 7.0 accelerometer event's reading, in g.</summary>
public class AccelerometerReadingEventArgs : EventArgs
{
    internal AccelerometerReadingEventArgs(double x, double y, double z, DateTimeOffset timestamp)
    {
        X = x;
        Y = y;
        Z = z;
        Timestamp = timestamp;
    }

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public DateTimeOffset Timestamp { get; }
}
