namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's per-gamer leaderboard writer, which only a gamer in a network session that writes
/// leaderboards owns (<see cref="Gamer.LeaderboardWriter"/> throws for every other gamer, as XNA's
/// base does). The C ABI does not expose CNA's session leaderboard writer yet, so no writer exists
/// to hand out; see docs/xna-compatibility.md.
/// </summary>
public sealed class LeaderboardWriter
{
    public LeaderboardWriter()
    {
    }

    public LeaderboardEntry GetLeaderboard(LeaderboardIdentity leaderboardId) =>
        throw new NotSupportedException("CNA's C ABI does not expose session leaderboard writing.");
}
