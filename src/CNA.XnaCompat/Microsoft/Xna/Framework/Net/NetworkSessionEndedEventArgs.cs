namespace Microsoft.Xna.Framework.Net;

public class NetworkSessionEndedEventArgs : EventArgs
{
    public NetworkSessionEndedEventArgs(NetworkSessionEndReason endReason)
    {
        EndReason = endReason;
    }

    public NetworkSessionEndReason EndReason { get; }
}
