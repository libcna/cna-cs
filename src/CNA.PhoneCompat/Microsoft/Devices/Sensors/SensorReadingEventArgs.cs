namespace Microsoft.Devices.Sensors;

public class SensorReadingEventArgs<T> : EventArgs
    where T : ISensorReading
{
    public SensorReadingEventArgs()
    {
    }

    public T SensorReading { get; set; } = default!;
}
