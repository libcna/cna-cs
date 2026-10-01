namespace Microsoft.Xna.Framework.Net;

public class GamerJoinedEventArgs : EventArgs
{
    public GamerJoinedEventArgs(NetworkGamer gamer)
    {
        Gamer = gamer;
    }

    public NetworkGamer Gamer { get; }
}
