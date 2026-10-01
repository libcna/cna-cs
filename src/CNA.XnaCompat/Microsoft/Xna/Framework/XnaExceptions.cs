using CNA.Interop;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Re-raises a native CNA failure as the exception the XNA call throws. Native names the canonical
/// exception it caught (ABI 0.37.0): the <c>System</c> ones are built by CNA.Interop, XNA's own here.
/// A failure native did not raise from an exception -- or one this table does not know -- stays a
/// <see cref="CNA.CnaException"/>, so nothing is guessed.
/// </summary>
internal static class XnaExceptions
{
    /// <summary>The exception <paramref name="type"/> names, or null when it is not a known one.</summary>
    internal static Exception? Create(string type, string paramName, string message) =>
        CnaCanonicalException.CreateSystem(type, paramName, message) ?? type switch
        {
            "Microsoft.Xna.Framework.Content.ContentLoadException" => new Content.ContentLoadException(message),
            "Microsoft.Xna.Framework.Audio.NoAudioHardwareException" => new Audio.NoAudioHardwareException(message),
            "Microsoft.Xna.Framework.Audio.NoMicrophoneConnectedException" => new Audio.NoMicrophoneConnectedException(message),
            "Microsoft.Xna.Framework.Audio.InstancePlayLimitException" => new Audio.InstancePlayLimitException(message),
            "Microsoft.Xna.Framework.Storage.StorageDeviceNotConnectedException" => new Storage.StorageDeviceNotConnectedException(message),
            "Microsoft.Xna.Framework.Graphics.DeviceLostException" => new Graphics.DeviceLostException(message),
            "Microsoft.Xna.Framework.Graphics.DeviceNotResetException" => new Graphics.DeviceNotResetException(message),
            "Microsoft.Xna.Framework.Graphics.NoSuitableGraphicsDeviceException" => new Graphics.NoSuitableGraphicsDeviceException(message),
            "Microsoft.Xna.Framework.GamerServices.GamerServicesNotAvailableException" => new GamerServices.GamerServicesNotAvailableException(message),
            "Microsoft.Xna.Framework.GamerServices.GameUpdateRequiredException" => new GamerServices.GameUpdateRequiredException(message),
            "Microsoft.Xna.Framework.GamerServices.GamerPrivilegeException" => new GamerServices.GamerPrivilegeException(message),
            "Microsoft.Xna.Framework.GamerServices.GuideAlreadyVisibleException" => new GamerServices.GuideAlreadyVisibleException(message),
            "Microsoft.Xna.Framework.GamerServices.NetworkNotAvailableException" => new GamerServices.NetworkNotAvailableException(message),
            "Microsoft.Xna.Framework.GamerServices.NetworkException" => new GamerServices.NetworkException(message),
            "Microsoft.Xna.Framework.Net.NetworkSessionJoinException" => new Net.NetworkSessionJoinException(message),
            _ => null,
        };

    /// <summary>The XNA exception behind a CNA.Framework failure, or the failure itself.</summary>
    internal static Exception Translate(CNA.CnaException failure) =>
        failure.CanonicalExceptionType is { } type
            ? Create(type, failure.CanonicalParamName ?? string.Empty, failure.NativeMessage ?? failure.Message) ?? failure
            : failure;

    /// <summary>Runs a facade call whose native failure XNA reports with its own exception type.</summary>
    internal static void Guard(Action call)
    {
        try
        {
            call();
        }
        catch (CNA.CnaException failure) when (Translate(failure) is var translated && !ReferenceEquals(translated, failure))
        {
            throw translated;
        }
    }

    internal static T Guard<T>(Func<T> call)
    {
        try
        {
            return call();
        }
        catch (CNA.CnaException failure) when (Translate(failure) is var translated && !ReferenceEquals(translated, failure))
        {
            throw translated;
        }
    }

    /// <summary>The XNA exception behind the calling thread's last native failure, read directly
    /// (for facades that call CNA.Interop themselves), or null.</summary>
    internal static Exception? FromLastNativeFailure(string message)
    {
        string type = CnaCanonicalException.LastType();
        return type.Length == 0 ? null : Create(type, CnaCanonicalException.LastParamName(), message);
    }
}
