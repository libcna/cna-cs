using CNA.Interop;

namespace Microsoft.Devices;

/// <summary>
/// The phone's vibration motor, as one process-wide controller. CNA drives the haptics of the
/// first device that has them; where there are none, starting and stopping are accepted and do
/// nothing, as on a phone with vibration disabled.
/// </summary>
public sealed class VibrateController
{
    private static readonly VibrateController s_default = new();

    private VibrateController()
    {
    }

    public static VibrateController Default => s_default;

    /// <summary>Vibrates for <paramref name="duration"/>, which must lie in [0, 5] seconds.</summary>
    public void Start(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero || duration > TimeSpan.FromSeconds(5))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        DevicesInterop.Check(
            Native.cna_vibrate_controller_start(DevicesInterop.RequireGame(nameof(Start)), duration.Ticks), nameof(Start));
    }

    /// <summary>Stops a vibration; with nothing running, or no game to address, it does nothing.</summary>
    public void Stop()
    {
        CnaHandle game = DevicesInterop.Game;
        if (!game.IsNull)
        {
            DevicesInterop.Check(Native.cna_vibrate_controller_stop(game), nameof(Stop));
        }
    }
}
