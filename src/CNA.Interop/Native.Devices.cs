using System.Runtime.InteropServices;

namespace CNA.Interop;

/// <summary>
/// P/Invoke surface of devices.h and sensors.h that the opt-in phone compatibility assembly
/// (CNA.PhoneCompat) is built on: the device environment, the vibration controller and the
/// accelerometer. Prototypes and ownership are documented in those headers; tools/abi-verify proves
/// each declaration against them.
/// </summary>
internal static partial class Native
{
    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_environment_get_device_type(out uint outDeviceType);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_vibrate_controller_start(CnaHandle game, long durationTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_vibrate_controller_stop(CnaHandle game);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_sensors_get_last_error_id_ext(out int outErrorId, out byte outHasErrorId);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_sensor_unsubscribe_ext(CnaHandle registration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_get_is_supported(CnaHandle game, out byte outSupported);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_create(CnaHandle game, out CnaHandle outSensor);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_get_state(CnaHandle sensor, out uint outState);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_start(CnaHandle sensor);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_stop(CnaHandle sensor);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_dispose(CnaHandle sensor);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_get_current_value(CnaHandle sensor, ref CnaAccelerometerReading outReading);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_get_is_data_valid(CnaHandle sensor, out byte outValid);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_get_time_between_updates_ticks(CnaHandle sensor, out long outTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_set_time_between_updates_ticks(CnaHandle sensor, long ticks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_subscribe_current_value_changed(CnaHandle sensor, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_subscribe_reading_changed(CnaHandle sensor, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_inject_synthetic_update_ext(CnaHandle sensor, float x, float y, float z);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_set_supported_for_tests_ext(CnaHandle sensor, byte supported);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_set_started_for_tests_ext(CnaHandle sensor, byte started);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_accelerometer_destroy(CnaHandle sensor);
}
