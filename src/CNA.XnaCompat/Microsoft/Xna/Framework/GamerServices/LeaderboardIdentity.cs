namespace Microsoft.Xna.Framework.GamerServices;

public struct LeaderboardIdentity
{
    public string Key { get; set; }

    public int GameMode { get; set; }

    /// <summary>XNA's own IL: the key is the <see cref="LeaderboardKey"/> member's name.</summary>
    public static LeaderboardIdentity Create(LeaderboardKey key, int gameMode) =>
        new() { Key = key.ToString(), GameMode = gameMode };

    public static LeaderboardIdentity Create(LeaderboardKey key) => Create(key, 0);
}
