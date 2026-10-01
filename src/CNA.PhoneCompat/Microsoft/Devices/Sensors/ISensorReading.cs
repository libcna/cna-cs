namespace Microsoft.Devices.Sensors;

/// <summary>A sensor reading: every one says when it was taken.</summary>
public interface ISensorReading
{
    DateTimeOffset Timestamp { get; }
}
