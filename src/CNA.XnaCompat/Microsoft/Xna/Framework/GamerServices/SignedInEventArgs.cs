namespace Microsoft.Xna.Framework.GamerServices;

public class SignedInEventArgs : EventArgs
{
    public SignedInEventArgs(SignedInGamer gamer)
    {
        Gamer = gamer;
    }

    public SignedInGamer Gamer { get; }
}
