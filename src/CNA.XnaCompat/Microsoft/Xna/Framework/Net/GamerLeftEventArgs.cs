namespace Microsoft.Xna.Framework.Net;

public class GamerLeftEventArgs : EventArgs
{
    public GamerLeftEventArgs(NetworkGamer gamer)
    {
        Gamer = gamer;
    }

    public NetworkGamer Gamer { get; }
}
