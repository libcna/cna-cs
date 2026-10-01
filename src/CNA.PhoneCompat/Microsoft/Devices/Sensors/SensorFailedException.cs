namespace Microsoft.Devices.Sensors;

/// <summary>A sensor refused to start; <see cref="ErrorId"/> is the platform's reason.</summary>
public class SensorFailedException : Exception
{
    internal SensorFailedException(string message, int errorId)
        : base(message)
    {
        ErrorId = errorId;
    }

    public int ErrorId { get; }
}

public class AccelerometerFailedException : SensorFailedException
{
    internal AccelerometerFailedException(string message, int errorId)
        : base(message, errorId)
    {
    }
}
