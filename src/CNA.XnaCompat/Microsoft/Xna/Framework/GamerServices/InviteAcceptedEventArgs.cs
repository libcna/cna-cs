namespace Microsoft.Xna.Framework.GamerServices;

public class InviteAcceptedEventArgs : EventArgs
{
    public InviteAcceptedEventArgs(SignedInGamer gamer, bool isCurrentSession)
    {
        Gamer = gamer;
        IsCurrentSession = isCurrentSession;
    }

    public bool IsCurrentSession { get; }

    public SignedInGamer Gamer { get; }
}
