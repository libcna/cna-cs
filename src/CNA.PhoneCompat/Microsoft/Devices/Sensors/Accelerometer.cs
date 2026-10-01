using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeResourceHandle = CNA.NativeResourceHandle;
using CNA.Interop;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors;

/// <summary>
/// The phone's accelerometer, over CNA's: a device without one -- most desktops -- reports
/// <see cref="SensorState.NotSupported"/>, and <see cref="Start"/> refuses with
/// <see cref="AccelerometerFailedException"/>, which is what phone games catch to fall back to other
/// input. Both the current reading event and the 7.0 <see cref="ReadingChanged"/> event are native's,
/// in native's order.
/// </summary>
public sealed unsafe class Accelerometer : SensorBase<AccelerometerReading>
{
    private readonly NativeResourceHandle _handle;
    private readonly GCHandle _self;
    private CnaHandle _currentValueRegistration;
    private CnaHandle _readingRegistration;

    public Accelerometer()
    {
        CnaHandle game = Microsoft.Devices.DevicesInterop.RequireGame(nameof(Accelerometer));
        Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_create(game, out CnaHandle sensor), nameof(Accelerometer));
        _handle = new NativeResourceHandle(sensor.Value, value => Native.cna_accelerometer_destroy(new CnaHandle(value)).IsSuccess());
        _self = GCHandle.Alloc(this, GCHandleType.Weak);
        nint context = GCHandle.ToIntPtr(_self);
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_subscribe_current_value_changed(
                sensor, (nint)(delegate* unmanaged[Cdecl]<CnaAccelerometerReading*, nint, void>)&OnCurrentValue, context,
                out _currentValueRegistration),
            nameof(CurrentValueChanged));
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_subscribe_reading_changed(
                sensor, (nint)(delegate* unmanaged[Cdecl]<CnaAccelerometerReadingEventInfo*, nint, void>)&OnReading, context,
                out _readingRegistration),
            nameof(ReadingChanged));
    }

    internal CnaHandle Handle
    {
        get
        {
            ThrowIfDisposed();
            return new CnaHandle(_handle.DangerousGetHandle());
        }
    }

    /// <summary>Whether this device has an accelerometer; false with no running game to ask.</summary>
    public static bool IsSupported
    {
        get
        {
            CnaHandle game = Microsoft.Devices.DevicesInterop.Game;
            if (game.IsNull)
            {
                return false;
            }

            Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_get_is_supported(game, out byte supported), nameof(IsSupported));
            return supported != 0;
        }
    }

    public SensorState State
    {
        get
        {
            Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_get_state(Handle, out uint state), nameof(State));
            return (SensorState)state;
        }
    }

    [Obsolete("Use CurrentValueChanged.")]
    public event EventHandler<AccelerometerReadingEventArgs> ReadingChanged = null!;

    public override void Start()
    {
        CnaResult result = Native.cna_accelerometer_start(Handle);
        if (result.IsSuccess())
        {
            return;
        }

        string message = CnaError.GetLastErrorMessage();
        if (Native.cna_sensors_get_last_error_id_ext(out int errorId, out byte hasErrorId).IsSuccess() && hasErrorId != 0)
        {
            throw new AccelerometerFailedException(string.IsNullOrEmpty(message) ? "The accelerometer could not start." : message, errorId);
        }

        Microsoft.Devices.DevicesInterop.Check(result, nameof(Start));
    }

    public override void Stop() => Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_stop(Handle), nameof(Stop));

    internal override AccelerometerReading ReadCurrentValue()
    {
        CnaAccelerometerReading reading = Versioned();
        Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_get_current_value(Handle, ref reading), nameof(CurrentValue));
        return FromNative(reading);
    }

    internal override bool ReadIsDataValid()
    {
        Microsoft.Devices.DevicesInterop.Check(Native.cna_accelerometer_get_is_data_valid(Handle, out byte valid), nameof(IsDataValid));
        return valid != 0;
    }

    internal override TimeSpan ReadTimeBetweenUpdates()
    {
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_get_time_between_updates_ticks(Handle, out long ticks), nameof(TimeBetweenUpdates));
        return new TimeSpan(ticks);
    }

    internal override void WriteTimeBetweenUpdates(TimeSpan value) =>
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_set_time_between_updates_ticks(Handle, value.Ticks), nameof(TimeBetweenUpdates));

    /// <summary>Unsubscribes, disposes the native sensor (which stops it) and releases it.</summary>
    internal override void ReleaseNative()
    {
        CnaHandle sensor = new(_handle.DangerousGetHandle());
        foreach (CnaHandle registration in new[] { _currentValueRegistration, _readingRegistration })
        {
            if (!registration.IsNull)
            {
                _ = Native.cna_sensor_unsubscribe_ext(registration);
            }
        }

        _currentValueRegistration = CnaHandle.Zero;
        _readingRegistration = CnaHandle.Zero;
        _ = Native.cna_accelerometer_dispose(sensor);
        _handle.Dispose();
        if (_self.IsAllocated)
        {
            _self.Free();
        }
    }

    /// <summary>Test seam: overrides the platform probe for this sensor (native's test route).</summary>
    internal void SetSupportedForTests(bool supported) =>
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_set_supported_for_tests_ext(Handle, supported ? (byte)1 : (byte)0), nameof(SetSupportedForTests));

    /// <summary>Test seam: marks the sensor started without opening hardware (native's test route).</summary>
    internal void SetStartedForTests(bool started) =>
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_set_started_for_tests_ext(Handle, started ? (byte)1 : (byte)0), nameof(SetStartedForTests));

    /// <summary>Test seam: feeds one reading in platform units (m/s^2), as a sensor would.</summary>
    internal void InjectForTests(float x, float y, float z) =>
        Microsoft.Devices.DevicesInterop.Check(
            Native.cna_accelerometer_inject_synthetic_update_ext(Handle, x, y, z), nameof(InjectForTests));

    private static CnaAccelerometerReading Versioned()
    {
        CnaAccelerometerReading reading = default;
        reading.StructSize = (uint)sizeof(CnaAccelerometerReading);
        reading.StructVersion = 1;
        return reading;
    }

    private static AccelerometerReading FromNative(in CnaAccelerometerReading reading) => new()
    {
        Acceleration = new Vector3(reading.Acceleration.X, reading.Acceleration.Y, reading.Acceleration.Z),
        Timestamp = Microsoft.Devices.DevicesInterop.ToDateTimeOffset(reading.Timestamp),
    };

    private static Accelerometer? From(nint context) =>
        context == 0 ? null : GCHandle.FromIntPtr(context).Target as Accelerometer;

    /// <summary>A handler's exception must not unwind into the platform's sensor thread, so the
    /// game gets it, as XNA would have surfaced it from the frame that ran the handler.</summary>
    private static void Report(Exception exception)
    {
        if (CNA.Game.Active is { } game)
        {
            game.ReportComponentFailure(exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnCurrentValue(CnaAccelerometerReading* reading, nint context)
    {
        try
        {
            From(context)?.RaiseCurrentValueChanged(FromNative(*reading));
        }
        catch (Exception exception)
        {
            Report(exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnReading(CnaAccelerometerReadingEventInfo* info, nint context)
    {
        try
        {
            if (From(context) is { } sensor)
            {
#pragma warning disable CS0618 // The 7.0 event is what phone games written for 7.0 subscribe to.
                sensor.ReadingChanged?.Invoke(
                    sensor,
                    new AccelerometerReadingEventArgs(info->X, info->Y, info->Z, Microsoft.Devices.DevicesInterop.ToDateTimeOffset(info->Timestamp)));
#pragma warning restore CS0618
            }
        }
        catch (Exception exception)
        {
            Report(exception);
        }
    }
}
