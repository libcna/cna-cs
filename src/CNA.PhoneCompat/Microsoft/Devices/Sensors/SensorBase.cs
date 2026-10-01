using CNA.Interop;

namespace Microsoft.Devices.Sensors;

/// <summary>
/// What every phone sensor shares: the current reading, whether it is real data yet, the update
/// interval, and the reading event. Native sensors deliver readings on the platform's sensor
/// thread, so <see cref="CurrentValueChanged"/> runs there, as it does on a phone.
/// </summary>
public abstract class SensorBase<TSensorReading> : IDisposable
    where TSensorReading : ISensorReading
{
    private int _disposed;

    internal SensorBase()
    {
    }

    ~SensorBase()
    {
        // The native sensor is released by its own handle's finalizer, on the game thread.
    }

    public TSensorReading CurrentValue => ReadCurrentValue();

    public bool IsDataValid => ReadIsDataValid();

    public TimeSpan TimeBetweenUpdates
    {
        get => ReadTimeBetweenUpdates();
        set => WriteTimeBetweenUpdates(value);
    }

    public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged = null!;

    internal bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    public abstract void Start();

    public abstract void Stop();

    /// <summary>A second dispose is a use after dispose, as the phone's sensor treats it.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        ReleaseNative();
        GC.SuppressFinalize(this);
    }

    internal void RaiseCurrentValueChanged(TSensorReading reading) =>
        CurrentValueChanged?.Invoke(this, new SensorReadingEventArgs<TSensorReading> { SensorReading = reading });

    internal abstract TSensorReading ReadCurrentValue();

    internal abstract bool ReadIsDataValid();

    internal abstract TimeSpan ReadTimeBetweenUpdates();

    internal abstract void WriteTimeBetweenUpdates(TimeSpan value);

    internal abstract void ReleaseNative();

    internal void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}
