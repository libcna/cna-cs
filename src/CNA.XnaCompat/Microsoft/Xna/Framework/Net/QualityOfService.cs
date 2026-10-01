using CNA.Interop;

namespace Microsoft.Xna.Framework.Net;

/// <summary>
/// A measured connection quality. XNA hands out one object per discovered session and refreshes it
/// in place while the measurement is still arriving, so this one is updated rather than replaced.
/// </summary>
public sealed class QualityOfService
{
    internal QualityOfService()
    {
    }

    private bool _isAvailable;
    private int _bytesPerSecondUpstream;
    private int _bytesPerSecondDownstream;
    private TimeSpan _averageRoundtripTime;
    private TimeSpan _minimumRoundtripTime;

    public bool IsAvailable => _isAvailable;

    public int BytesPerSecondUpstream => _bytesPerSecondUpstream;

    public int BytesPerSecondDownstream => _bytesPerSecondDownstream;

    public TimeSpan AverageRoundtripTime => _averageRoundtripTime;

    public TimeSpan MinimumRoundtripTime => _minimumRoundtripTime;

    internal void Update(in CnaQualityOfService value)
    {
        _isAvailable = value.IsAvailable != 0;
        _bytesPerSecondUpstream = value.BytesPerSecondUpstream;
        _bytesPerSecondDownstream = value.BytesPerSecondDownstream;
        _averageRoundtripTime = new TimeSpan(value.AverageRoundtripTicks);
        _minimumRoundtripTime = new TimeSpan(value.MinimumRoundtripTicks);
    }
}
