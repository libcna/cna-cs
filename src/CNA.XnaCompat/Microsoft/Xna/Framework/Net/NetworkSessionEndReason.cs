namespace Microsoft.Xna.Framework.Net;

public enum NetworkSessionEndReason
{
    ClientSignedOut = 0,
    HostEndedSession = 1,
    RemovedByHost = 2,
    Disconnected = 3,
}
