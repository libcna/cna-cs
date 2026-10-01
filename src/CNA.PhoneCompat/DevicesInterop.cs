using CNA.Interop;

namespace Microsoft.Devices;

/// <summary>What the phone facade shares on its way to the C ABI.</summary>
internal static class DevicesInterop
{
    /// <summary>The running CNA game, which every device route is addressed through, or zero.</summary>
    internal static CnaHandle Game => CNA.Game.Active is { } game ? new CnaHandle(game.NativeHandle) : CnaHandle.Zero;

    internal static CnaHandle RequireGame(string operation)
    {
        CnaHandle game = Game;
        if (game.IsNull)
        {
            throw new InvalidOperationException($"{operation} needs a running CNA game.");
        }

        return game;
    }

    /// <summary>A native refusal as the exception the phone API throws for the same category.</summary>
    internal static void Check(CnaResult result, string operation)
    {
        if (result.IsSuccess())
        {
            return;
        }

        string detail = CnaError.GetLastErrorMessage();
        string message = string.IsNullOrEmpty(detail) ? $"{operation} failed with {result}." : detail;
        throw result switch
        {
            CnaResult.InvalidArgument => new ArgumentException(message),
            CnaResult.InvalidState => new InvalidOperationException(message),
            CnaResult.NotSupported => new NotSupportedException(message),
            CnaResult.InvalidHandle => new ObjectDisposedException(operation, message),
            _ => new InvalidOperationException(message),
        };
    }

    internal static DateTimeOffset ToDateTimeOffset(in CnaDateTimeOffset value)
    {
        try
        {
            return new DateTimeOffset(value.Ticks, TimeSpan.FromTicks(value.OffsetTicks));
        }
        catch (ArgumentException)
        {
            // Native's default reading carries no meaningful time; DateTimeOffset's own default is
            // what the canonical default reading reports.
            return default;
        }
    }
}
