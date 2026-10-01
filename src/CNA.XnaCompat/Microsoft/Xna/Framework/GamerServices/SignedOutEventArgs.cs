namespace Microsoft.Xna.Framework.GamerServices;

public class SignedOutEventArgs : EventArgs
{
    public SignedOutEventArgs(SignedInGamer gamer)
    {
        Gamer = gamer;
    }

    public SignedInGamer Gamer { get; }
}
