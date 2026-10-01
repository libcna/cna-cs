namespace Microsoft.Xna.Framework.Net;

/// <summary>XNA constructs these itself; there is no public constructor.</summary>
public sealed class WriteLeaderboardsEventArgs : EventArgs
{
    internal WriteLeaderboardsEventArgs(NetworkGamer gamer, bool isLeaving)
    {
        Gamer = gamer;
        IsLeaving = isLeaving;
    }

    public NetworkGamer Gamer { get; }

    public bool IsLeaving { get; }
}
